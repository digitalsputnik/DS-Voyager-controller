using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using VoyagerApp.Lamps;
using VoyagerApp.Lamps.Voyager;
using VoyagerApp.Utilities;

namespace VoyagerApp.Networking.Voyager
{
    public class VoyagerClient : LampClient
    {
        public const int PORT_DISCOVERY = 30000;
        public const int PORT_SETTINGS = 30001;
        public const int PORT_VIDEO = 30002;

        public const float POLL_INTERVAL = 1f;

        public delegate void ConnectionHandler();
        public event ConnectionHandler onConnectionChanged;

        public Dictionary<Lamp, Dictionary<string, (int, byte[])>> keepSending = new Dictionary<Lamp, Dictionary<string, (int, byte[])>>();

        RudpClient discovery;
        RudpClient settings;

        OffsetService offset;

        public VoyagerClient(MonoBehaviour behaviour)
        {
            discovery = new RudpClient(PORT_DISCOVERY);
            settings = new RudpClient(PORT_SETTINGS);
            settings.onInitialize += () => onConnectionChanged?.Invoke();

            offset = new OffsetService();

            Application.quitting += offset.Dispose;

            behaviour.StartCoroutine(IEnumPollLoop());
        }

        public double TimeOffset => offset.Offset.TotalSeconds;

        public void KeepSendingPacket(Lamp lamp, string key, Packet packet, int port, double timestamp)
        {
            var data = SendPacket(lamp, packet, port, timestamp);
            if (!keepSending.ContainsKey(lamp))
                keepSending.Add(lamp, new Dictionary<string, (int, byte[])>());
            keepSending[lamp][key] = (port, data);
        }

        public byte[] SendPacket(Lamp lamp, Packet packet, int port)
        {
            if (packet.op == OpCode.Collection)
            {
                var collection = (PacketCollection)packet;
                for (int i = 0; i < collection.packets.Length; i++)
                {
                    collection.packets[i].timestamp = TimeUtils.Epoch;
                    collection.packets[i].serial = lamp.serial;
                }
                packet.serial = lamp.serial;
                packet = collection;
            }

            byte[] data = packet.Serialize();
            IPEndPoint endpoint = new IPEndPoint(lamp.address, port);
            if (WorkspaceUtils.Lamps.Contains(lamp) && lamp.connected)
                Send(data, endpoint);
            return data;
        }

        public byte[] SendPacket(Lamp lamp, Packet packet, int port, double timestamp)
        {
            if (packet.op == OpCode.Collection)
            {
                var collection = (PacketCollection)packet;
                for (int i = 0; i < collection.packets.Length; i++)
                {
                    collection.packets[i].timestamp = timestamp;
                    collection.packets[i].serial = lamp.serial;
                }
                packet = collection;
                packet.serial = lamp.serial;
            }
            byte[] data = packet.Serialize(timestamp);
            IPEndPoint endpoint = new IPEndPoint(lamp.address, port);
            if (WorkspaceUtils.Lamps.Contains(lamp) && lamp.connected)
                Send(data, endpoint);
            return data;
        }

        public void TurnToClient(Lamp lamp, string ssid, string password)
        {
            var package = VoyagerNetworkMode.Client(ssid, password, lamp.serial);
            var endpoint = new IPEndPoint(lamp.address, PORT_DISCOVERY);
            Send(package.ToData(), endpoint);
        }

        public void TurnToRouter(Lamp lamp)
        {
            var package = VoyagerNetworkMode.Router(lamp.serial);
            var endpoint = new IPEndPoint(lamp.address, PORT_DISCOVERY);
            Send(package.ToData(), endpoint);
        }

        public void TurnToMaster(Lamp lamp)
        {
            var package = VoyagerNetworkMode.Master(lamp.serial);
            var endpoint = new IPEndPoint(lamp.address, PORT_DISCOVERY);
            Send(package.ToData(), endpoint);
        }

        public override void Send(byte[] data, object info)
        {
            IPEndPoint endpoint = (IPEndPoint)info;
            discovery.Send(endpoint, data);
        }

        public override void Receive()
        {
            ReceiveClient(discovery);
            ReceiveClient(settings);
        }

        public void GetSsidListFromLamp(Lamp lamp, Action<Lamp, string[]> received, float timeout)
        {
            new Thread(() => GetSsidListFromLampThread(lamp, received, timeout)).Start();
        }

        void GetSsidListFromLampThread(Lamp lamp, Action<Lamp, string[]> received, float timeout)
        {
            IPEndPoint endpoint = new IPEndPoint(lamp.address, PORT_DISCOVERY);

            onReceived += OnReceived;

            bool ssidsReceived = false;
            double starttime = TimeUtils.Epoch;

            Packet packet = new SsidListRequestPacket();
            while (!ssidsReceived && (TimeUtils.Epoch - starttime) < timeout)
            {
                SendPacket(lamp, packet, PORT_DISCOVERY);
                Thread.Sleep(100);
            }

            onReceived -= OnReceived;

            void OnReceived(object sender, byte[] data)
            {

                endpoint = (IPEndPoint)sender;
                var deserialized = Packet.Deserialize<SsidListResponseResponse>(data);

                if (deserialized != null && deserialized.op == OpCode.SsidListResponse)
                {
                    if (!string.IsNullOrEmpty(deserialized.serial))
                    {
                        if (deserialized.serial == lamp.serial)
                        {
                            MainThread.Dispach(() => received?.Invoke(lamp, deserialized.ssids));
                            ssidsReceived = true;
                        }
                    }
                    else if (endpoint.Address.ToString() == lamp.address.ToString())
                    {
                        MainThread.Dispach(() => received?.Invoke(lamp, deserialized.ssids));
                        ssidsReceived = true;
                    }
                }
            }
        }

        void ReceiveClient(RudpClient client)
        {
            while (client.Available > 0)
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

                byte[] data = client.Receive(ref sender);
                string json = Encoding.UTF8.GetString(data);

                if (IsValidJson(json))
                { 
                    LampManager manager = LampManager.instance;
                    try
                    {
                        JObject obj = JObject.Parse(json);
                        if (obj["serial"] != null)
                        {
                            string serial = (string)obj["serial"];
                            if (!string.IsNullOrEmpty(serial))
                                manager.GetLampWithSerial(serial)?.PushData(data);
                            else
                                manager.GetLampWithAddress(sender.Address)?.PushData(data);
                        }
                        else
                            manager.GetLampWithAddress(sender.Address)?.PushData(data);

                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning(ex);
                    }
                }
                InvokeReceived(sender, data);
            }
        }

        IEnumerator IEnumPollLoop()
        {
            yield return null;

            while (true)
            {
                PollDmx();
                DiscoverLamps();
                DiscoverLampsOp();
                SendKeepPackets();
                yield return new WaitForSeconds(POLL_INTERVAL);
            }
        }

        void DiscoverLamps()
        {
            byte[] message = { 0xD5, 0x0A, 0x80, 0x10 };
            var endpoint = new IPEndPoint(
                NetUtils.BroadcastAddress,
                PORT_DISCOVERY);
            Send(message, endpoint);
        }

        void DiscoverLampsOp()
        {
            var packet = new PollRequestPacket();
            var data = packet.Serialize();
            var endpoint = new IPEndPoint(
                NetUtils.BroadcastAddress,
                PORT_DISCOVERY);
            Send(data, endpoint);
        }

        void PollDmx()
        {
            var packet = new DmxModeRequest();
            var data = packet.Serialize();
            foreach (var lamp in LampManager.instance.Lamps)
            {
                IPEndPoint endpoint = new IPEndPoint(lamp.address, PORT_SETTINGS);
                Send(data, endpoint);
            }
        }

        void SendKeepPackets()
        {
            foreach (var lamp in keepSending.Keys.ToArray())
            {
                if (!WorkspaceUtils.Lamps.Contains(lamp))
                {
                    keepSending.Remove(lamp);
                    continue;
                }

                if (lamp.connected)
                {
                    foreach (var key in keepSending[lamp].Keys)
                    {
                        int port = keepSending[lamp][key].Item1;
                        byte[] data = keepSending[lamp][key].Item2;
                        IPEndPoint endpoint = new IPEndPoint(lamp.address, port);
                        Send(data, endpoint);
                    }
                }
            }
        }


        bool IsValidJson(string strInput)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{", StringComparison.Ordinal) && strInput.EndsWith("}", StringComparison.Ordinal)) || //For object
                (strInput.StartsWith("[", StringComparison.Ordinal) && strInput.EndsWith("]", StringComparison.Ordinal)))   //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    Console.WriteLine(jex.Message);
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}

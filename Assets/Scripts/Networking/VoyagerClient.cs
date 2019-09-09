using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using UnityEngine;
using VoyagerApp.Dmx;
using VoyagerApp.Lamps;
using VoyagerApp.Lamps.Voyager;
using VoyagerApp.Networking.Packages;
using VoyagerApp.Utilities;

namespace VoyagerApp.Networking
{
    public class VoyagerClient : LampClient
    {
        public const int  DISCOVERY_PORT = 30000;
        public const float DISCOVERY_INTERVAL = 0.2f;

        public const int SETTINGS_PORT = 30001;

        public const float DMX_POLL_INTERVAL = 2.0f;

        public const int VIDEO_PORT = 30002;

        RudpClient discovery;
        RudpClient dmx;
        RudpClient video;

        OffsetService offset;

        public VoyagerClient(MonoBehaviour behaviour)
        {
            discovery = new RudpClient(DISCOVERY_PORT);
            dmx = new RudpClient(SETTINGS_PORT);
            video = new RudpClient(VIDEO_PORT);

            offset = new OffsetService();

            behaviour.StartCoroutine(IEnumDiscoveryLoop());
            behaviour.StartCoroutine(IEnumDmxPollLoop());
        }

        public double TimeOffset => offset.Offset.TotalSeconds;

        public void SendPlaymode(Lamp lamp, VoyagerPlaybackMode mode)
        {
            double time = TimeUtils.Epoch + TimeOffset;
            var container = new VoyagerPlaybackPackage(mode, time);
            IPEndPoint endpoint = new IPEndPoint(lamp.address, SETTINGS_PORT);
            Send(container.ToData(), endpoint);
        }

        public void SendPacket(Lamp lamp, Packet packet)
        {
            byte[] data = packet.Serialize();
            IPEndPoint endpoint = new IPEndPoint(lamp.address, SETTINGS_PORT);
            Send(data, endpoint);
        }

        //public void SendVideoMetadata(Lamp lamp)
        //{
        //    if (lamp.video != null)
        //    {
        //        double time = TimeUtils.Epoch;
        //        lamp.video.lastTimestamp = time;
        //        var container = VideoFrameMetadata.FromVideo(
        //            lamp.video,
        //            lamp.itshe,
        //            time,
        //            TimeOffset);

        //        IPEndPoint endpoint = new IPEndPoint(lamp.address, SETTINGS_PORT);
        //        Send(container.ToData(), endpoint);
        //    }
        //}

        //public void SendItshAsMetadata(Lamp lamp)
        //{
        //    double time = TimeUtils.Epoch;
        //    var container = VideoFrameMetadata.FromVideo(
        //        null,
        //        lamp.itshe,
        //        time,
        //        TimeOffset);

        //    IPEndPoint endpoint = new IPEndPoint(lamp.address, SETTINGS_PORT);
        //    Send(container.ToData(), endpoint);
        //}

        //public void SendFpsAsMetadata(Lamp lamp)
        //{
        //    double time = TimeUtils.Epoch;
        //    var container = VideoFrameMetadata.FromVideo(
        //        lamp.video,
        //        lamp.itshe,
        //        time,
        //        TimeOffset);

        //    IPEndPoint endpoint = new IPEndPoint(lamp.address, SETTINGS_PORT);
        //    Send(container.ToData(), endpoint);
        //}

        public void TurnToClient(Lamp lamp, string ssid, string password)
        {
            var package = VoyagerNetworkMode.Client(ssid, password, lamp.serial);
            var endpoint = new IPEndPoint(lamp.address, DISCOVERY_PORT);
            Send(package.ToData(), endpoint);
        }

        public void TurnToRouter(Lamp lamp)
        {
            var package = VoyagerNetworkMode.Router(lamp.serial);
            var endpoint = new IPEndPoint(lamp.address, DISCOVERY_PORT);
            Send(package.ToData(), endpoint);
        }

        public void TurnToMaster(Lamp lamp)
        {
            var package = VoyagerNetworkMode.Master(lamp.serial);
            var endpoint = new IPEndPoint(lamp.address, DISCOVERY_PORT);
            Send(package.ToData(), endpoint);
        }

        public void SendVideoFrameData(Lamp lamp, long frame, Color32[] colors)
        {
            var container = VideoFrameData.FromColors(frame, colors);
            byte[] data = container.ToData();
            IPEndPoint endpoint = new IPEndPoint(lamp.address, VIDEO_PORT);
            Send(data, endpoint);
        }

        public override void Send(byte[] data, object info)
        {
            IPEndPoint endpoint = (IPEndPoint)info;
            discovery.Send(endpoint, data);
        }

        public override void Receive()
        {
            ReceiveClient(discovery);
            ReceiveClient(dmx);
        }

        public void SendDmxSettings(IPAddress address, DmxSettings settings)
        {
            IPEndPoint endpoint = new IPEndPoint(address, SETTINGS_PORT);
            dmx.Send(endpoint, settings.ToData());
        }

        public void GetSsidListFromLamp(Lamp lamp, Action<string[]> received, float timeout)
        {
            new Thread(() => GetSsidListFromLampThread(lamp, received, timeout)).Start();
        }

        void GetSsidListFromLampThread(Lamp lamp, Action<string[]> received, float timeout)
        {
            byte[] sendMessage = { 0xD5, 0x0A, 0x87, 0x10 };
            IPEndPoint endpoint = new IPEndPoint(lamp.address, DISCOVERY_PORT);

            UdpClient client = new UdpClient();
            for (int i = 0; i < 5; i++)
                client.Send(sendMessage, sendMessage.Length, endpoint);

            bool ssidsReceived = false;
            double starttime = TimeUtils.Epoch;
            while (!ssidsReceived && (TimeUtils.Epoch - starttime) < timeout)
            {
                while (client.Available > 0)
                {
                    var data = client.Receive(ref endpoint);
                    var json = Encoding.UTF8.GetString(data);
                    var ssidsForLamp = JsonConvert.DeserializeObject<List<string>>(json);
                    MainThread.Dispach(() => received?.Invoke(ssidsForLamp.ToArray()));
                    ssidsReceived = true;
                }
                Thread.Sleep(10);
            }

            client.Dispose();
        }

        void ReceiveClient(RudpClient client)
        {
            while (client.Available > 0)
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref sender);
                InvokeReceived(sender, data);
            }
        }

        IEnumerator IEnumDiscoveryLoop()
        {
            while (true)
            {
                Discover();
                yield return new WaitForSeconds(DISCOVERY_INTERVAL);
            }
        }

        void Discover()
        {
            byte[] message = { 0xD5, 0x0A, 0x80, 0x10 };
            var endpoint = new IPEndPoint(NetUtils.BroadcastAddress,
                                          DISCOVERY_PORT);
            Send(message, endpoint);
        }

        IEnumerator IEnumDmxPollLoop()
        {
            while (true)
            {
                PollDmx();
                yield return new WaitForSeconds(DMX_POLL_INTERVAL);
            }
        }

        void PollDmx()
        {
            byte[] data = new DmxPoll().ToData();
            foreach (var lamp in LampManager.instance.Lamps)
            {
                IPEndPoint endpoint = new IPEndPoint(lamp.address, SETTINGS_PORT);
                Send(data, endpoint);
            }
        }
    }
}

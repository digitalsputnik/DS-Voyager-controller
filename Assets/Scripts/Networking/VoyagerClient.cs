﻿using System;
using System.Collections;
using System.Collections.Generic;
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
        public const bool DEBUG = true;

        public const int  DISCOVERY_PORT = 30000;
        public const float DISCOVERY_INTERVAL = 1f;

        public const int SETTINGS_PORT = 30001;

        public const float DMX_POLL_INTERVAL = 2.0f;

        public const int VIDEO_PORT = 30002;

        RudpClient discovery;
        RudpClient dmx;

        OffsetService offset;

        public VoyagerClient(MonoBehaviour behaviour)
        {
            discovery = new RudpClient(DISCOVERY_PORT);
            dmx = new RudpClient(SETTINGS_PORT);

            offset = new OffsetService();

            behaviour.StartCoroutine(IEnumDiscoveryLoop());
            behaviour.StartCoroutine(IEnumDmxPollLoop());
        }

        public double TimeOffset => offset.Offset.TotalSeconds;

        public void SendPacket(Lamp lamp, Packet packet)
        {
            if (packet.op == OpCode.Collection)
            {
                var collection = (PacketCollection)packet;
                for (int i = 0; i < collection.packets.Length; i++)
                    collection.packets[i].timestamp = TimeUtils.Epoch;
                packet = collection;
            }
            byte[] data = packet.Serialize();
            IPEndPoint endpoint = new IPEndPoint(lamp.address, SETTINGS_PORT);
            Send(data, endpoint);
        }

        public void SendPacket(Lamp lamp, Packet packet, double timestamp)
        {
            if (packet.op == OpCode.Collection)
            {
                var collection = (PacketCollection)packet;
                for (int i = 0; i < collection.packets.Length; i++)
                    collection.packets[i].timestamp = timestamp;
                packet = collection;
            }
            byte[] data = packet.Serialize(timestamp);
            IPEndPoint endpoint = new IPEndPoint(lamp.address, SETTINGS_PORT);
            Send(data, endpoint);
        }

        public void SendPacketToVideoPort(Lamp lamp, Packet packet, double time)
        {
            byte[] data = packet.Serialize(time);
            IPEndPoint endpoint = new IPEndPoint(lamp.address, VIDEO_PORT);
            Send(data, endpoint);
        }

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

        public override void Send(byte[] data, object info)
        {
            if (DEBUG) Debug.Log(Encoding.UTF8.GetString(data));

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
                LampManager.instance.GetLampWithAddress(sender.Address)?.PushData(data);
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

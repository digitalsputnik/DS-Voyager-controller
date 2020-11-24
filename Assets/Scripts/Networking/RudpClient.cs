using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using VoyagerApp.Utilities;

namespace VoyagerApp.Networking
{
    public class RudpClient
    {
        public delegate void InitializeHandler();
        public event InitializeHandler onInitialize;

        private UdpClient client;
        private readonly int port;
        private bool ready;

        public bool EnableBroadcast
        {
            get => client.EnableBroadcast;
            set => client.EnableBroadcast = value;
        }

        public bool ReuseAddress
        {
            get => (bool)client.Client.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress);
            set => client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, value);
        }

        public int Available => client.Available;

        public RudpClient(int port)
        {
            this.port = port;
            InitializeClient();
        }

        public void Send(IPEndPoint endpoint, byte[] data)
        {
            if (!ready) return;

            try
            {
                client.Send(data, data.Length, endpoint);
            }
            catch (Exception)
            {
                ready = false;
                client.Dispose();
                client = null;
                InitializeClient();
            }
        }

        public byte[] Receive(ref IPEndPoint endpoint)
        {
            if (!ready) return new byte[0];

            try
            {
                return client.Receive(ref endpoint);
            }
            catch (Exception)
            {
                ready = false;
                client.Dispose();
                client = null;
                InitializeClient();
                return new byte[0];
            }
        }

        public void Close() => client.Close();

        void InitializeClient()
        {
            var address = NetUtils.WifiInterfaceAddress;
            var endpoint = new IPEndPoint(address, port);
            client = new UdpClient(endpoint) { Client = { ReceiveTimeout = 1000 }};
            ready = true;
            onInitialize?.Invoke();
        }
    }
}
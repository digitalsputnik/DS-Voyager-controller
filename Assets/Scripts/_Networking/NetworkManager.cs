﻿using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.NetworkInformation;

namespace Voyager.Networking
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager instance;

        [Tooltip("Time in seconds, that the network manager reads incoming messages")]
        [SerializeField] float NetworkReadInterval = 0.5f;

		UdpClient sendingClient = new UdpClient();

		UdpClient client30000;
		UdpClient client31000;

		List<UdpClient> Clients = new List<UdpClient>();

		public delegate void LampDataReceivedHandler(string response, IPAddress ip);
		public delegate void LampColorDataReceivedHandler(byte[] data, IPAddress ip);

		public static event LampDataReceivedHandler OnAvailableLampsResponse;
		public static event LampDataReceivedHandler OnLampSceneResponse;
		public static event LampDataReceivedHandler OnLampSsidListResponse;
		public static event LampColorDataReceivedHandler OnLampColorResponse;

        void Awake()
        {
            if (instance == null)
            {
                DontDestroyOnLoad(this);
                instance = this;
                Init();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Init()
        {
            InvokeRepeating("ReadClient", 0.0f, NetworkReadInterval);
			NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;
			SetupClients();
        }

		void SetupClients()
		{
			client30000 = new UdpClient(30000);
			client30000.EnableBroadcast = true;
			Clients.Add(client30000);



			var socket31000 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            var endPoint31000 = new IPEndPoint(IPAddress.Any, 31000);
			socket31000.Bind(endPoint31000);
			socket31000.ReceiveBufferSize = 1024;
			socket31000.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client31000 = new UdpClient();
            //client31000.ExclusiveAddressUse = false;
            client31000.Client = socket31000;
			Clients.Add(client31000);
		}

		void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {
			//Clients.Clear();
			//client30000 = null;
			//SetupClients();
        }
        
        void ReadClient()
        {         
			foreach(UdpClient client in Clients)
			{
				IPEndPoint receivalEndpoint = new IPEndPoint(IPAddress.None, 0);
                
				while(client.Available > 0)
				{
					byte[] receivedBytes = client.Receive(ref receivalEndpoint);
					if(receivedBytes.Length != 0)
						StartCoroutine(ParseResponse(receivedBytes, receivalEndpoint));
				}
			}
        }
        
		IEnumerator ParseResponse(byte[] receivedBytes, IPEndPoint endpoint)
		{
			string receivedString = Encoding.UTF8.GetString(receivedBytes);
			IPAddress ip = endpoint.Address;

            if(receivedString.StartsWith("{", StringComparison.Ordinal))
            {            
                var jObj = JObject.Parse(receivedString);

                if (jObj["battery_level"] != null)
                {
                    LampDataReceivedHandler handler = OnAvailableLampsResponse;
					if (handler != null) handler(receivedString, ip);
                }
                else if (jObj["TimeStamp"] != null)
                {
                    LampDataReceivedHandler handler = OnLampSceneResponse;
					if (handler != null) handler(receivedString, ip);
                }
			}
			else if (receivedString.StartsWith("[", StringComparison.Ordinal))
			{
				LampDataReceivedHandler handler = OnLampSsidListResponse;
				if (handler != null) handler(receivedString, ip);
			}
			else if (endpoint.Port == 31000)
			{
				LampColorDataReceivedHandler handler = OnLampColorResponse;
				if (handler != null) handler(receivedBytes, ip);
			}

			yield return null;
		}

        public static void AskAvailableLamps()
        {
            AskAvailableLamps(1);
        }

		public static void AskAvailableLamps(int pollTimes)
        {
			IPEndPoint sendingEndpoint = new IPEndPoint(IPAddress.Broadcast, 30000);
            byte[] message = { 0xD5, 0x0A, 0x80, 0x10 };

			for (int i = 0; i < pollTimes; i++)
				instance.client30000.Send(message, message.Length, sendingEndpoint);
        }

		public static void AskLampLayers(IPAddress ip)
		{
			AskLampLayers(1, ip);
		}

		public static void AskLampLayers(int pollTimes, IPAddress ip)
		{
			//int port = 30001;
			//IPEndPoint sendingEndpoint = new IPEndPoint(ip, port);
			//string jsonData = JsonConvert.SerializeObject(new LayerPollPackage { PollLayers = true });
			//byte[] message = Encoding.ASCII.GetBytes(jsonData);
			////IPEndPoint sendingEndpoint = new IPEndPoint(IPAddress.Any, port);

			//Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			//socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			//socket.Bind(new IPEndPoint(IPAddress.Any, port));

			//UdpClient client = instance.GetClient(sendingEndpoint, socket);

			//for (int i = 0; i < pollTimes; i++)
                //client.Send(message, message.Length, sendingEndpoint);
        }

		public static void AskLampSsidList(IPAddress ip)
		{
			AskLampSsidList(1, ip);
		}

		public static void AskLampSsidList(int pollTimes, IPAddress ip)
		{
			//int port = 30000;
   //         IPEndPoint sendingEndpoint = new IPEndPoint(IPAddress.Broadcast, port);
			//byte[] message = { 0xD5, 0x0A, 0x87, 0x10 };
			//PollMessage(pollTimes, sendingEndpoint, message);
		}

		public static void AskTurnApMode(IPAddress ip, int channel, string ssid, string password)
		{
			AskTurnApMode(1, ip, channel, ssid, password);
		}

		public static void AskTurnApMode(int pollTimes, IPAddress ip, int channel, string ssid, string password)
		{
            IPEndPoint sendingEndpoint = new IPEndPoint(ip, 30000);
			ApModePackage package = new ApModePackage
			{
				set_channel = channel,
				set_ssid = ssid,
				set_password = password
			};
			string jsonData = JsonConvert.SerializeObject(package);
			byte[] message = Encoding.UTF8.GetBytes(jsonData);
			PollMessage(pollTimes, message, sendingEndpoint, instance.client30000);
		}

		public static void AskTurnClientMode(IPAddress ip, string ssid, string password)
		{
			AskTurnClientMode(1, ip, ssid, password);
		}

		public static void AskTurnClientMode(int pollTimes, IPAddress ip, string ssid, string password)
		{
            IPEndPoint sendingEndpoint = new IPEndPoint(ip, 30000);
			ClientModePackage package = new ClientModePackage
			{
				set_pattern = ssid,
				set_pattern_ps = password
            };
            string jsonData = JsonConvert.SerializeObject(package);
            byte[] message = Encoding.UTF8.GetBytes(jsonData);
			PollMessage(pollTimes, message, sendingEndpoint, instance.client30000);
		}

		public static void AskColorData()
		{
			AskColorData(IPAddress.Any);
		}

		public static void AskColorData(IPAddress ip)
		{
			AskColorData(1, ip);
		}

		public static void AskColorData(int pollTimes, IPAddress ip)
		{
			IPEndPoint sendingEndpoint = new IPEndPoint(ip, 31000);
			byte[] message = { 0 };

			for (int i = 0; i < pollTimes; i++)
				instance.sendingClient.Send(message, message.Length, sendingEndpoint);
		}

		public static void PollMessage(int times, byte[] message, IPEndPoint endPoint, UdpClient client)
		{
			for (int i = 0; i < times; i++)
				client.Send(message, message.Length, endPoint);
		}
    }

	[Serializable]
    public struct ReplyUdpResponse
	{
		public byte[] IP { get; set; }
		public int length { get; set; }
		public int battery_level { get; set; }
		public int[] BQ_temp { get; set; }
		public int CHIP_temp { get; set; }
		public int[] charging_status { get; set; }
		public int[] LPC_version { get; set; }
		public int[] CHIP_version { get; set; }
		public int[] animation_version { get; set; }
		public string MAC_last6 { get; set; }
		public string passive_active_mode { get; set; }
		public string serial_name { get; set; }
		public int hardware_version { get; set; }
		public string active_mode { get; set; }
        public string active_pattern { get; set; }
        public string active_pattern_ps { get; set; }
        public int active_channel { get; set; }
        public string active_ssid { get; set; }
        public string active_password { get; set; }
	}

	[Serializable]
    public struct LayerPollPackage
	{
		public bool PollLayers;
	}

	[Serializable]
	public class ApModePackage
    {
        public string network_mode = "ap_mode";
        public int set_channel { get; set; }
        public string set_ssid { get; set; }
        public string set_password { get; set; }
    }

	[Serializable]
    public class ClientModePackage
    {
        public string network_mode = "client_mode";
        public string set_pattern { get; set; }
        public string set_pattern_ps { get; set; }
    }
}
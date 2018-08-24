using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.NetworkInformation;
using System.Linq;

namespace Voyager.Networking
{
	public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager instance;

        [Tooltip("Time in seconds, that the network manager reads incoming messages")]
        [SerializeField] float NetworkReadInterval = 0.5f;
		[SerializeField] bool Debugging;

		UdpClient sendingClient;
        
		OffsetService offsetService;
		UdpClient client30000;
		UdpClient client30001;
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
			SetupClients();
        }

		void SetupClients()
		{
			IPEndPoint sendingEndpoint = new IPEndPoint(GetWifiInterfaceAddress(), 0);
			Socket sendingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			sendingSocket.Bind(sendingEndpoint);
			sendingClient = new UdpClient();
			sendingClient.Client = sendingSocket;

			IPEndPoint endPoint30000 = new IPEndPoint(GetWifiInterfaceAddress(), 30000);
			Socket socket30000 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			socket30000.Bind(endPoint30000);
			client30000 = new UdpClient();
            client30000.Client = socket30000;
			client30000.EnableBroadcast = true;
            Clients.Add(client30000);

			IPEndPoint endPoint30001 = new IPEndPoint(GetWifiInterfaceAddress(), 30001);
            Socket socket30001 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket30001.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			socket30001.Bind(endPoint30001);
			client30001 = new UdpClient();
			client30001.Client = socket30001;
			Clients.Add(client30001);

			IPEndPoint endPoint31000 = new IPEndPoint(GetWifiInterfaceAddress(), 31000);
            Socket socket31000 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket31000.Bind(endPoint31000);
			client31000 = new UdpClient();
			client31000.Client = socket31000;
			client31000.Client.ReceiveBufferSize = 1024;
			Clients.Add(client31000);

			offsetService = new OffsetService();
		}
        
        void ReadClient()
        {         
			foreach(UdpClient client in Clients)
			{
				IPEndPoint receivalEndpoint = new IPEndPoint(IPAddress.Any, 0);
                
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
			IPEndPoint sendingEndpoint = new IPEndPoint(ip, 30001);
			LayerPollPackage pollPackage = new LayerPollPackage { PollLayers = true };
			string jsonData = JsonConvert.SerializeObject(pollPackage);
			byte[] message = Encoding.ASCII.GetBytes(jsonData);

			for (int i = 0; i < pollTimes; i++)
				instance.client30001.Send(message, message.Length, sendingEndpoint);
        }

		public static void AskLampSsidList(IPAddress ip)
		{
			AskLampSsidList(1, ip);
		}

		public static void AskLampSsidList(int pollTimes, IPAddress ip)
		{
			int port = 30000;
            IPEndPoint sendingEndpoint = new IPEndPoint(ip, port);
			byte[] message = { 0xD5, 0x0A, 0x87, 0x10 };

			for (int i = 0; i < pollTimes; i++)
                instance.sendingClient.Send(message, message.Length, sendingEndpoint);
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
			Debug.Log("Asking for color data");
                     
			for (int i = 0; i < pollTimes; i++)
				instance.sendingClient.Send(new byte[] { 0 }, 1, sendingEndpoint);
		}
        
		public static void RegisterDevice(IPAddress ip)
		{
			IPEndPoint sendingEndpoint = new IPEndPoint(ip, 30001);
			string jsonData = JsonConvert.SerializeObject(new RegisterDevicePackage() { RegisterDevice = true });
            byte[] message = Encoding.ASCII.GetBytes(jsonData);
			instance.sendingClient.Send(message, message.Length, sendingEndpoint);
		}

		public static void SetDetectionMode(IPAddress ip, bool detection)
		{
			SetDetectionMode(ip, detection, 1);
		}

		public static void SetDetectionMode(IPAddress ip, bool detection, int pollTimes)
        {
			IPEndPoint sendingEndpoint = new IPEndPoint(ip, 30001);
			DetectionModePackage package = new DetectionModePackage { DetectionMode = detection };
            string jsonData = JsonConvert.SerializeObject(package);
            byte[] message = Encoding.UTF8.GetBytes(jsonData);
			PollMessage(pollTimes, message, sendingEndpoint, instance.sendingClient);
        }

		public static double GetTimesyncOffset()
		{
			return instance.offsetService.Offset.TotalSeconds;
		}

		public static void PollMessage(int times, byte[] message, IPEndPoint endPoint, UdpClient client)
		{
			for (int i = 0; i < times; i++)
				client.Send(message, message.Length, endPoint);
		}
              
		public void SendMessage(IPAddress ip, byte[] message)
		{
			IPEndPoint endPoint = new IPEndPoint (ip, 30000);
			instance.sendingClient.Send (message, message.Length, endPoint);
		}

		public static void SendMessage(IPEndPoint endPoint, byte[] message)
		{
			instance.sendingClient.Send(message, message.Length, endPoint);
		}
        
		public static void SendVideoStream(IPEndPoint endPoint, byte[] message)
		{
			UdpClient client = new UdpClient(new IPEndPoint(GetWifiInterfaceAddress(), 0));
			if (instance.Debugging) Debug.Log("[VIDEO STREAM] Sending to " + endPoint + ": " + Encoding.ASCII.GetString(message));
			client.Send(message, message.Length, endPoint);
		}

        public static IPAddress GetWifiInterfaceAddress()
		{
			if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            {
                NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();

                if (adapters.Length > 1)
                {
                    NetworkInterface WirelessInterface = adapters.Where(x => x.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 &&
                                                                        x.SupportsMulticast && x.OperationalStatus == OperationalStatus.Up && 
    					                                                x.GetIPProperties().GetIPv4Properties() != null).FirstOrDefault();
                    if (WirelessInterface != null)
                    {
                        byte[] addressBytes = WirelessInterface.GetIPProperties().UnicastAddresses
        						                               .Where(x => x.Address.AddressFamily == AddressFamily.InterNetwork)
        						                               .FirstOrDefault().Address.GetAddressBytes();
                        return new IPAddress(addressBytes);
                    }
                }
            }

            return IPAddress.Any;
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
    public struct DetectionModePackage
	{
		public bool DetectionMode;
	}

	[Serializable]
	public struct RegisterDevicePackage
	{
		public bool RegisterDevice;
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
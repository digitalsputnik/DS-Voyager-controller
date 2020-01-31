// -----------------------------------------------------------------
// Author: Taavet Maask	Date: 7/30/2019
// Copyright: © Digital Sputnik OÜ
// -----------------------------------------------------------------

using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UnityEngine;
using VoyagerApp.Networking;
using VoyagerApp.Networking.Voyager;

namespace VoyagerApp.Utilities
{
    public static class NetUtils
    {
        public static VoyagerClient VoyagerClient
        {
            get => NetworkManager.instance.GetLampClient<VoyagerClient>();
        }

        public static IPAddress WifiInterfaceAddress
        {
            get
            {
                if (Application.platform == RuntimePlatform.WindowsPlayer ||
                    Application.platform == RuntimePlatform.WindowsEditor ||
                    Application.platform == RuntimePlatform.OSXPlayer ||
                    Application.platform == RuntimePlatform.OSXEditor )
                {
                    var wireless = WirelessInterface;
                    if (wireless != null)
                        return InterfaceToAddress(wireless);
                }

                return IPAddress.Any;
            }
        }

        public static IPAddress BroadcastAddress
        {
            get
            {
                return IPAddress.Broadcast;

                //if (Application.platform == RuntimePlatform.Android)
                //    return AndroidBroadcastAddress;
            }
        }

        public static IPAddress LocalIPAddress
        {
            get
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                        return ip;
                }
                return IPAddress.Any;
            }
        }

        public static IPAddress[] LocalIPAddresses
        {
            get
            {
                return Dns.GetHostEntry(Dns.GetHostName())
                    .AddressList
                    .Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                    .ToArray();
            }
        }

        static IPAddress AndroidBroadcastAddress
        {
            get
            {
                byte[] address = LocalIPAddress.GetAddressBytes();
                byte[] subnet = { 255, 255, 255, 0 };
                byte[] broadcast = new byte[address.Length];

                for (int i = 0; i < broadcast.Length; i++)
                    broadcast[i] = (byte)(address[i] | subnet[i] ^ 255);

                return new IPAddress(broadcast);
            }
        }

        static NetworkInterface WirelessInterface
        {
            get
            {
                var adapters = NetworkInterface.GetAllNetworkInterfaces();
                var wireless = adapters.FirstOrDefault(_ =>
                    _.SupportsMulticast &&
                    _.OperationalStatus == OperationalStatus.Up &&
                    _.GetIPProperties().GetIPv4Properties() != null &&
                    _.NetworkInterfaceType == NetworkInterfaceType.Wireless80211);
                return wireless;
            }
        }

        static IPAddress InterfaceToAddress(NetworkInterface _interface)
        {
            var addresses = _interface.GetIPProperties().UnicastAddresses;
            var info = addresses.First(_ =>
                _.Address.AddressFamily == AddressFamily.InterNetwork);
            byte[] address = info.Address.GetAddressBytes();
            return new IPAddress(address);
        }
    }
}
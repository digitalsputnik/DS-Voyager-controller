// -----------------------------------------------------------------
// Author: Taavet Maask	Date: 7/30/2019
// Copyright: © Digital Sputnik OÜ
// -----------------------------------------------------------------

using System;
using System.Collections.Generic;
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
                if (Application.platform != RuntimePlatform.WindowsPlayer &&
                    Application.platform != RuntimePlatform.WindowsEditor &&
                    Application.platform != RuntimePlatform.OSXPlayer &&
                    Application.platform != RuntimePlatform.OSXEditor)
                    return IPAddress.Any;
                
                var wireless = NetworkInterface;
                return wireless != null ? InterfaceToAddress(wireless) : IPAddress.Any;
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
                List<IPAddress> addresses = new List<IPAddress>();
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        //if (!ip.IsDnsEligible)
                        //    continue;
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            addresses.Add(ip.Address);
                    }
                }
                return addresses.ToArray();
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

        static NetworkInterface NetworkInterface
        {
            get
            {
                var adapters = NetworkInterface.GetAllNetworkInterfaces();

                var networkInterfaces = adapters.Where(_ =>
                _.SupportsMulticast &&
                _.OperationalStatus == OperationalStatus.Up &&
                _.Description != "Npcap Loopback Adapter" &&
                (_.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || _.NetworkInterfaceType == NetworkInterfaceType.Ethernet));

                var wirelessInterface = networkInterfaces.FirstOrDefault(_ => _.NetworkInterfaceType == NetworkInterfaceType.Wireless80211);
                var wiredInterface = networkInterfaces.FirstOrDefault(_ => _.NetworkInterfaceType == NetworkInterfaceType.Ethernet);

                if (wirelessInterface != null)
                {
                    if (GetProperties(wirelessInterface))
                        return wirelessInterface;
                }
                else if (wiredInterface != null)
                {
                    if (GetProperties(wiredInterface))
                        return wiredInterface;
                }

                return null;
            }
        }

        static bool GetProperties(NetworkInterface networkInterface)
        {
            bool interfaceFound = false;

            try
            {
                if (networkInterface.GetIPProperties().GetIPv4Properties() != null)
                {
                    interfaceFound = true;
                }
            }
            catch
            {
                try
                {
                    if (networkInterface.GetIPProperties().GetIPv6Properties() != null)
                    {
                        interfaceFound = true;
                    }
                }
                catch
                {
                        interfaceFound = false;
                }
            }

            return interfaceFound;
        }

        static IPAddress InterfaceToAddress(NetworkInterface _interface)
        {
            var addresses = _interface.GetIPProperties().UnicastAddresses;
            var info = addresses.FirstOrDefault(addr => addr.Address.AddressFamily == AddressFamily.InterNetwork);

            if (info != null)
            {
                byte[] address = info.Address.GetAddressBytes();
                return new IPAddress(address);
            }

            return IPAddress.Any;
        }
    }
}
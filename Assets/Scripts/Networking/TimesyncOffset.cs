using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using GuerrillaNtp_DS;
using Newtonsoft.Json;
using VoyagerApp.Utilities;

namespace VoyagerApp.Networking
{
    public class OffsetService
    {
        public readonly int UpdateInterval;
        public TimeSpan Offset = TimeSpan.Zero;

        readonly Thread thread;

        public OffsetService()
        {
            UpdateInterval = 150000;
            thread = new Thread(MasterFinder);
            thread.Start();
        }

        public void Dispose()
        {
            thread.Interrupt();
        }

        void MasterFinder()
        {
            while (true)
            {
                TimeSpan receivedOffset = OffsetServiceClient.Offset;
                if (receivedOffset != TimeSpan.Zero) Offset = receivedOffset;
                Thread.Sleep(UpdateInterval);
            }
        }
    }

    static class OffsetServiceClient
    {
        public static TimeSpan LastOffset = TimeSpan.MinValue;

        public static TimeSpan Offset
        {
            get
            {
                try
                {
                    IPEndPoint endpoint = MasterEndpoint;
                    if (endpoint == null)
                        return LastOffset;

                    using (var ntp = new NtpClient(endpoint))
                    {
                        if (ntp.GetCorrectionOffset().Equals(TimeSpan.Zero))
                            return LastOffset;
                        return ntp.GetCorrectionOffset();
                    }
                }
                catch { return TimeSpan.Zero; }
            }
        }

        public static TimeSpan OffsetOf(IPEndPoint ntpServer)
        {
            try
            {
                using (var ntp = new NtpClient(ntpServer))
                    return ntp.GetCorrectionOffset();
            }
            catch { return TimeSpan.Zero; }
        }

        static IPEndPoint MasterEndpoint
        {
            get
            {
                int timeoutMilliseconds = 4100;
                int port = 51259;
                IPAddress address = NetUtils.WifiInterfaceAddress;

                var ad = new IPEndPoint(address, port);
                var listener = new UdpClient(ad);

                listener.Client.SetSocketOption(SocketOptionLevel.Socket,
                                                SocketOptionName.ReuseAddress,
                                                true);
                listener.Client.ReceiveTimeout = timeoutMilliseconds;
                listener.Client.EnableBroadcast = true;

                var server = new IPEndPoint(address, port);
                var sw = new Stopwatch();

                var message = new byte[] { 1, 2, 3, 4 };
                var dest = new IPEndPoint(IPAddress.Broadcast, port);

                listener.Send(message, message.Length, dest);

                sw.Start();

                try
                {
                    while (timeoutMilliseconds > sw.ElapsedMilliseconds ||
                           listener.Available > 0)
                    {
                        var announce = listener.Receive(ref server);
                        var announceString = Encoding.ASCII.GetString(announce);
                        if (!ValidateAnnounce(announceString)) continue;
                        server.Port = 123;
                        return server;
                    }
                }
                finally { listener.Close(); }

                return null;
            }
        }

        static bool ValidateAnnounce(string message)
        {
            try
            {
                var a = JsonConvert.DeserializeObject<List<string>>(message);
                if (a[6] == "VoyagerSync" && Convert.ToInt32(a[0]) > 0)
                    return true;
            }
            catch { return false; }

            return false;
        }
    }
}
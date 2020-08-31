using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using GuerrillaNtp_DS;
using Newtonsoft.Json;
using VoyagerApp.UI;
using VoyagerApp.Utilities;
using Debug = UnityEngine.Debug;

namespace VoyagerApp.Networking
{
    public class OffsetService
    {
        public readonly int UpdateInterval;
        public TimeSpan Offset = TimeSpan.Zero;

        private readonly Thread thread;

        public OffsetService()
        {
            UpdateInterval = 1000;
            thread = new Thread(MasterFinder);
            thread.Start();
        }

        public void Dispose()
        {
            thread.Interrupt();
            thread.Abort();
        }

        private void MasterFinder()
        {
            while (true)
            {
                var receivedOffset = OffsetServiceClient.Offset;
                if (receivedOffset != TimeSpan.Zero) Offset = receivedOffset;
                Thread.Sleep(UpdateInterval);
            }
        }
    }

    internal static class OffsetServiceClient
    {
        private static TimeSpan _lastOffset = TimeSpan.MinValue;

        public static TimeSpan Offset
        {
            get
            {
                try
                {
                    var endpoint = MasterEndpoint;

                    if (endpoint == null)
                    {
                        MainThread.Dispach(() =>
                        {
                            if (ApplicationState.DeveloperMode)
                                Debug.LogError("Endpoint wasn't found!");
                        });
                        
                        return _lastOffset;
                    }

                    using (var ntp = new NtpClient(endpoint))
                    {
                        if (ntp.GetCorrectionOffset().Equals(TimeSpan.Zero))
                            return _lastOffset;

                        var value = ntp.GetCorrectionOffset();
                        _lastOffset = value;
                        return value;
                    }
                }
                catch (Exception ex)
                {
                    MainThread.Dispach(() =>
                    {
                        if (ApplicationState.DeveloperMode)
                            Debug.LogError(ex.Message);
                    });

                    return _lastOffset;
                }
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

        private static IPEndPoint MasterEndpoint
        {
            get
            {
                const int TIMEOUT_MILLISECONDS = 4100;
                const int PORT = 51259;

                var listener = new UdpClient();
                var server = new IPEndPoint(IPAddress.Any, PORT);
                var dest = new IPEndPoint(NetUtils.WifiInterfaceAddress, PORT);
                var message = new byte[] { 1, 2, 3, 4 };
                
                listener.Client.Bind(server);
                listener.Send(message, message.Length, dest);

                var sw = new Stopwatch();
                sw.Start();

                try
                {
                    while (sw.ElapsedMilliseconds < TIMEOUT_MILLISECONDS)
                    {
                        while (listener.Available > 0)
                        {
                            var endpoint = new IPEndPoint(IPAddress.Any, 0);
                            var announce = listener.Receive(ref endpoint);
                            var announceString = Encoding.ASCII.GetString(announce);

                            if (!ValidateAnnounce(announceString)) continue;

                            endpoint.Port = 123;
                            return endpoint;
                        }
                        
                        Thread.Sleep(10);
                    }
                }
                catch (Exception ex)
                {
                    MainThread.Dispach(() =>
                    {
                        if (ApplicationState.DeveloperMode)
                            Debug.LogError(ex.Message);
                    });
                }
                finally
                {
                    listener.Close();
                }

                return null;
            }
        }

        private static bool ValidateAnnounce(string message)
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
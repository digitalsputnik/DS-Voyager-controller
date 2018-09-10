using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using GuerrillaNtp_DS;
using System.Net;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Text;

namespace Voyager.Networking
{
	class OffsetService : MonoBehaviour
    {

        readonly Thread _th;
        public TimeSpan Offset = TimeSpan.Zero;
        public int RefreshRateMilliseconds;

        public OffsetService()
        {
            RefreshRateMilliseconds = 150000;
            _th = new Thread(MasterFinder);
            _th.Start();
        }

        public void Dispose()
        {
            _th.Interrupt();
        }

        void MasterFinder()
        {
            while (true)
            {
				TimeSpan receivedOffset = OffsetServiceClient.GetOffset();
                if (receivedOffset != TimeSpan.Zero) Offset = receivedOffset;
                Thread.Sleep(RefreshRateMilliseconds);
            }
        }
    }


    static class OffsetServiceClient
    {
		public static TimeSpan LastOffset;

        public static TimeSpan GetOffset()
        {
            try
            {
				IPEndPoint endPoint = FindMasterIp();
				if(endPoint == null)
				{
					UnityEngine.Debug.LogError("[TIMEOFFSET] Master IP == null");
					return LastOffset;
				}
				else
				{
					using (var ntp = new NtpClient(endPoint))
                    {
						if (ntp.GetCorrectionOffset().Equals(TimeSpan.Zero))
						{
							UnityEngine.Debug.LogError("[TIMEOFFSET] Time offset == 0");
							return LastOffset;
						}
						else
							return ntp.GetCorrectionOffset();
                    }	
				}
            }
            catch (Exception) { return TimeSpan.Zero; }
        }

        public static TimeSpan GetOffset(IPEndPoint ntpServer)
        {
            try
            {
                using (var ntp = new NtpClient(ntpServer))
                {
                    return ntp.GetCorrectionOffset();
                }
            }
            catch (Exception) { return TimeSpan.Zero; }
        }

        static IPEndPoint FindMasterIp()
        {
            const int timeoutMilliseconds = 4100;
            const int announcePort = 51259;
            var ad = new IPEndPoint(IPAddress.Any, announcePort);
            var listener = new UdpClient(ad);
            listener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            listener.Client.ReceiveTimeout = timeoutMilliseconds;
            listener.Client.EnableBroadcast = true;
            // listener.Client.Connect(ad);

            var server = new IPEndPoint(IPAddress.Any, announcePort);

            var sw = new Stopwatch();
            // No timeout if timeout equals zero 
			sw.Start();

            try
            {
                while (timeoutMilliseconds > sw.ElapsedMilliseconds)
                {
                    byte[] announce = listener.Receive(ref server);
                    string announceString = Encoding.ASCII.GetString(announce);
                    if (!ValidateAnnounce(announceString)) continue;
                    server.Port = 123;
                    return server;
                }

				return null;
            }
            finally { listener.Close(); }
        }

        static bool ValidateAnnounce(string announceString)
        {
            try
            {
                var a = JsonConvert.DeserializeObject<List<string>>(announceString);
                if (a[6] == "VoyagerSync" && Convert.ToInt32(a[0]) > 0) return true;
            }
            catch { return false; }

            return false;
        }
    }
}
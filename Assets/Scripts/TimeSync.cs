using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

class PTPDTO
{
    public double Timestamp { get; set; }
    public string Response { get; set; }
}

public class TimeSync : MonoBehaviour {

    public AnimationSender AnimationControl;
    public double TimeOffset = 0;
    public double LampTime;

    public string IP = "172.20.0.1";
    public int port = 2468;
    public IPEndPoint SlaveEndpoint;
    public int numberOfTimes = 5;

    private IPEndPoint localEndpoint;

    private UdpClient udpClient;

    // Use this for initialization
    void Start () {
        SlaveEndpoint = new IPEndPoint(IPAddress.Parse(IP), port);

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        if (localEndpoint == null)
        {
            SetLocalEndpoint();
            //return;
        }
        udpClient = new UdpClient(localEndpoint);
#else
        udpClient = new UdpClient();
#endif
        udpClient.Client.ReceiveTimeout = 10000; // Timeout = 10s

        //Start coroutine to timesync each minute
        //TODO: Sync with each lamp individually
        StartCoroutine("TimeSyncCoroutine");
	}

    void Update()
    {
        LampTime = GetTime() + TimeOffset;
    }

    private double GetTime()
    {
        return (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
    }

    private void SetLocalEndpoint()
    {
        NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
        var WirelessInterface = adapters.Where(x => x.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 && x.SupportsMulticast && x.OperationalStatus == OperationalStatus.Up && x.GetIPProperties().GetIPv4Properties() != null).FirstOrDefault();
        var localIP = WirelessInterface.GetIPProperties().UnicastAddresses.Where(x => x.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).FirstOrDefault().Address.Address;
        localEndpoint = new IPEndPoint(localIP, 0);
    }

    /// <summary>
    /// Sends message to slave.
    /// </summary>
    /// <param name="dataString"></param>
    double SendData(string dataString)
    {
        byte[] data = Encoding.ASCII.GetBytes(dataString);
        udpClient.Send(data, data.Length, SlaveEndpoint);
        return GetTime();
    }

    PTPDTO ReceiveData()
    {
        PTPDTO returnData = new PTPDTO();
        //if (udpClient.Available == 0)
        //{
        //    return new PTPDTO { Response = "0", Timestamp = 0 };
        //}
        var receivedData = new byte[] { };
        try { 
            receivedData = udpClient.Receive(ref SlaveEndpoint);
        }
        catch (Exception)
        {
            UnityEngine.Debug.Log("Time sync failed due to receive timeout. Will try again!");
        }

        returnData.Timestamp = GetTime();
        returnData.Response = Encoding.ASCII.GetString(receivedData);
        //returnData.Response = GetTime().ToString(); // Encoding.ASCII.GetString(receivedData);
        return returnData;
    }

    double SyncPacket()
    {
        var t1 = SendData("sync_packet");
        PTPDTO p = ReceiveData();
        return Convert.ToDouble(p.Response) - t1;
    }

    double DelayPacket()
    {
        SendData("delay_packet");
        PTPDTO p = ReceiveData();
        return p.Timestamp - Convert.ToDouble(p.Response);
    }

    /// <summary>
    /// Coroutine, which synchronizes data between lamp and UI
    /// </summary>
    /// <returns></returns>
    IEnumerator TimeSyncCoroutine()
    {
        while (true)
        {
            //Not to overload the coroutine of sync packet is 0
            yield return new WaitForSeconds(5f);

            List<double> Offsets = new List<double>();
            List<double> Delays = new List<double>();

            if (SendData("sync") == 0)
            {
                continue;
            }

            /*
             * This part is for timeout TODO: Coroutine timeout handling
            Stopwatch sw = new Stopwatch();
            bool TimeoutExceeded = false;
            while (udpClient.Available == 0 && !TimeoutExceeded)
            {
                if(sw.ElapsedMilliseconds > 2000)
                {
                    TimeoutExceeded = true;
                }
                yield return null;
            }
            if (TimeoutExceeded)
            {
                TimeoutExceeded = false;
                continue;
            }
            */

            PTPDTO p1 = ReceiveData();

            SendData(numberOfTimes.ToString());
            PTPDTO p2 = ReceiveData();

            if (true)//p2.Response == "ready")
            {
                for (int i = 0; i < numberOfTimes; i++)
                {
                    //TODO: Add a queue number to each request to foolproof for missing packets
                    var MasterSlaveDifference = SyncPacket();
                    var SlaveMasterDifference = DelayPacket();

                    Offsets.Add((MasterSlaveDifference - SlaveMasterDifference) / 2);
                    Delays.Add((MasterSlaveDifference + SlaveMasterDifference) / 2);

                    SendData("next");
                }

                UnityEngine.Debug.Log("Offset delta " + (Offsets.Max() - Offsets.Min()));
                UnityEngine.Debug.Log("Delay delta " + (Delays.Max() - Delays.Min()));
                TimeOffset = Offsets.Sum() / Offsets.Count + Delays.Sum() / Delays.Count;

                //Sets the offset time for calculation
                AnimationControl.TimeOffset = TimeOffset;
            }

            yield return new WaitForSeconds(55f);
        }
    }
}

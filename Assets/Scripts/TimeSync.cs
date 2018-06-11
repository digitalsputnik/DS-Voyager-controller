using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
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

        //Start coroutine to timesync each minute
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
        //    return new PTPDTO {Response = "0", Timestamp = 0};
        //}
        while (udpClient.Available == 0)
        {
            //TODO: Add timeout after which 0-s is returned
            continue;
        }
        var receivedData = udpClient.Receive(ref SlaveEndpoint);
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
            List<double> Offsets = new List<double>();
            List<double> Delays = new List<double>();

            if (SendData("sync") == 0)
            {
                yield return new WaitForSeconds(5f);
                continue;
            }
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

                Debug.Log("Offset delta " + (Offsets.Max() - Offsets.Min()));
                Debug.Log("Delay delta " + (Delays.Max() - Delays.Min()));
                TimeOffset = Offsets.Sum() / Offsets.Count + Delays.Sum() / Delays.Count;

                //Sets the offset time for calculation
                AnimationControl.TimeOffset = TimeOffset;
            }

            //TODO: Waiting time should be dependent on the quality of sync data (standard deviation) and if the sync has failed or not.
            
            yield return new WaitForSeconds(60f);
        }
    }
}

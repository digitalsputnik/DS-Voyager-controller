using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Text;

public class LampColorProperties
{
    public string IP { get; set; }
    public int LampLength { get; set; }
    public byte[][] ColorPattern { get; set; }
    public string MacName { get; set; }
    public int BatteryLevel { get; set; }
}

public class UDPpacketDTO
{
    public UdpClient client { get; set; }
    public IPEndPoint endpoint { get; set; }
    public byte[] data { get; set; }
    public int length { get; set; }
}

public class LampCommunication: MonoBehaviour {

    public List<LampColorProperties> lampColors = new List<LampColorProperties>();

    public Text DebugText;

    public Dictionary<IPAddress, int> LampIPtoLengthDictionary = new Dictionary<IPAddress, int>();
    Dictionary<IPAddress, int> NewLampIPtoLengthDictionary = new Dictionary<IPAddress, int>();
    public int permutationCounter { get; set; }
    public List<List<byte[]>> ColorPermutations { get; set; }
    public List<byte[]> CalibrationColors { get; set; }

    Dictionary<IPAddress, ExtraProperties> IPtoProps = new Dictionary<IPAddress, ExtraProperties>();

    IPEndPoint localEndpoint;

    // Use this for initialization
    void Start ()
    {
        SetupPermutations();

        //Find available lamps in network
        //TODO: Use a better way to get this gameobject!
        var tempLampDict = GameObject.Find("DetectedLampProperties").GetComponent<DetectedLampProperties>().LampIPtoLengthDictionary;
        LampIPtoLengthDictionary = new Dictionary<IPAddress, int>(tempLampDict);
        NewLampIPtoLengthDictionary = new Dictionary<IPAddress, int>(tempLampDict);

        //LampIPtoLengthDictionary = GetAllAvailableLamps();

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        //Get local wireless endpoint
        GetWirelessEndpoint();
#endif

        //Destination
        UdpClient PollClient = new UdpClient();
        PollClient.EnableBroadcast = true;
        int Port = 30000;
        byte[] message = new byte[] { 0xD5, 0x0A, 0x80, 0x10 };

        UDPpacketDTO pollPacket = new UDPpacketDTO
        {
            client = PollClient,
            data = message,
            endpoint = new IPEndPoint(IPAddress.Broadcast, Port),
            length = message.Length
        };

        //Start polling lamps
        StartCoroutine("PollLamps", pollPacket);

        //DebugText.text = "Adding short Voyager for individual connection!";
        //Short
        //LampIPtoLengthDictionary.Add(IPAddress.Parse("172.20.0.1"), 0);
        //Long
        //LampIPtoLengthDictionary.Add(IPAddress.Parse("172.20.0.1"), 1);

        permutationCounter = 0;

        //DebugText.text = "Creating connections!";
        //lampColors = new List<LampColorProperties>();
        //foreach (var lamp in LampIPtoLengthDictionary)
        //{
        //    //Coloring
        //    var colorPattern = ColorPermutations[permutationCounter].ToArray();
        //    int colorCount = CalibrationColors.Count();
        //    UDPpacketDTO cDTO = GenerateColorPacket(lamp.Key, lamp.Value, colorPattern, colorCount);

        //    StartCoroutine("SendMessage", cDTO);

        //    lampColors.Add(new LampColorProperties
        //    {
        //        IP = lamp.Key.ToString(),
        //        LampLength = lamp.Value,
        //        ColorPattern = colorPattern
        //    });

        //    permutationCounter++;
        //    if (permutationCounter >= ColorPermutations.Count())
        //        break;
        //}
    }

    private void GetWirelessEndpoint()
    {
        NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
        var WirelessInterface = adapters.Where(x => x.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 && x.SupportsMulticast && x.OperationalStatus == OperationalStatus.Up && x.GetIPProperties().GetIPv4Properties() != null).FirstOrDefault();
        var localIP = WirelessInterface.GetIPProperties().UnicastAddresses.Where(x => x.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).FirstOrDefault().Address.Address;
        localEndpoint = new IPEndPoint(localIP, 0);
    }

    public void SetupPermutations()
    {
        //Calibration colors
        CalibrationColors = new List<byte[]>();
        CalibrationColors.Add(new byte[] { 100, 0, 0, 0 });
        CalibrationColors.Add(new byte[] { 100, 100, 0, 0 });
        CalibrationColors.Add(new byte[] { 0, 100, 0, 0 });
        CalibrationColors.Add(new byte[] { 0, 0, 100, 0 });

        //Get irreversible permutations
        ColorPermutations = GetIrreversiblePermutations(CalibrationColors).ToList();
    }

    void Update()
    {
        //Check for new lights
        SendPermutationsToNewLamps(new Dictionary<IPAddress, int>(NewLampIPtoLengthDictionary));
    }

    public void SendPermutationsToNewLamps(Dictionary<IPAddress,int> lampIPtoLengthDictionary)
    {
        if (lampIPtoLengthDictionary.Count > 0)
        {
            foreach (var newLamp in lampIPtoLengthDictionary)
            {
                if (permutationCounter >= ColorPermutations.Count())
                    break;

                //Ignore DS's
                if (newLamp.Value < 20)
                {
                    continue;
                }

                //Start sending color permutation to lamp
                var colorPattern = ColorPermutations[permutationCounter].ToArray();

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                if (localEndpoint == null)
                {
                    GetWirelessEndpoint();
                    return;
                }
#endif

                UDPpacketDTO cDTO = GenerateColorPacket(newLamp.Key, newLamp.Value, colorPattern, CalibrationColors.Count());

                if (NewLampIPtoLengthDictionary == null)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        SendUDPMessage(cDTO);
                        Thread.Sleep(50);
                    }
                }
                else
                {
                    StartCoroutine("SendMessage", cDTO);
                }


                //Add lamp for detection
                if (IPtoProps.ContainsKey(newLamp.Key))
                {
                    lampColors.Add(new LampColorProperties
                    {
                        IP = newLamp.Key.ToString(),
                        LampLength = newLamp.Value,
                        ColorPattern = colorPattern,
                        BatteryLevel = IPtoProps[newLamp.Key].BatteryLevel,
                        MacName = IPtoProps[newLamp.Key].LampMac
                    });
                }

                //Remove lamp from new lamps
                if (NewLampIPtoLengthDictionary != null)
                {
                    if (NewLampIPtoLengthDictionary.Keys.Contains(newLamp.Key))
                    {
                        NewLampIPtoLengthDictionary.Remove(newLamp.Key);
                    }
                }

                permutationCounter++;
            }
        }
    }

    private UDPpacketDTO GenerateColorPacket(IPAddress lampIP, int lampLength, byte[][] colorPattern, int colorCount)
    {
        //Send color code to lamp
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        var client = new UdpClient(localEndpoint);
#else
        var client = new UdpClient();
#endif

        var remoteEndPoint = new IPEndPoint(lampIP, 30000);

        byte[] data = new byte[343];
        byte[] lightValues = new byte[332];
        //header
        //RGBW
        System.Buffer.BlockCopy(new byte[] { 0xD5, 0x0A, 0x10, 0x03 }, 0, data, 0, 4);

        var cDTO = new UDPpacketDTO();

        //General color sending!
        System.Buffer.BlockCopy(new byte[] { 0x00, 0x00, 0x00, Convert.ToByte(lampLength) }, 0, data, 4, 4);
        //terminator + empty checksum
        System.Buffer.BlockCopy(new byte[] { 0xEF, 0xFE, 0x00 }, 0, data, 8 + 4*lampLength, 3);

        lightValues = SetLightValueArray(lightValues, colorPattern, lampLength, colorCount);

        System.Buffer.BlockCopy(lightValues, 0, data, 8, 4*lampLength);

        cDTO = new UDPpacketDTO
        {
            data = data,
            client = client,
            endpoint = remoteEndPoint,
            length = 8 + 4 * lampLength + 3
        };

        /*
        //Short
        if (lampLength == 0)
        {
            //start & finish pixels
            System.Buffer.BlockCopy(new byte[] { 0x00, 0x00, 0x00, 0x27 }, 0, data, 4, 4);
            //terminator + empty checksum
            System.Buffer.BlockCopy(new byte[] { 0xEF, 0xFE, 0x00 }, 0, data, 164, 3);

            int pixelCount = 39;
            lightValues = SetLightValueArray(lightValues, colorPattern, pixelCount, colorCount);

            System.Buffer.BlockCopy(lightValues, 0, data, 8, 156);

            cDTO = new UDPpacketDTO
            {
                data = data,
                client = client,
                endpoint = remoteEndPoint,
                length = 167
            };
        }

        //Long
        if (lampLength == 1)
        {
            //start & finish pixels
            System.Buffer.BlockCopy(new byte[] { 0x00, 0x00, 0x00, 0x52 }, 0, data, 4, 4);
            //terminator + empty checksum
            System.Buffer.BlockCopy(new byte[] { 0xEF, 0xFE, 0x00 }, 0, data, 340, 3);

            int pixelCount = 82;
            lightValues = SetLightValueArray(lightValues, colorPattern, pixelCount, colorCount);

            System.Buffer.BlockCopy(lightValues, 0, data, 8, 332);

            cDTO = new UDPpacketDTO
            {
                data = data,
                client = client,
                endpoint = remoteEndPoint,
                length = data.Length
            };
        }
        */
        return cDTO;
    }

    IEnumerator SendMessage(UDPpacketDTO udpDTO)
    {
        var animsender = new AnimationSender();

        while (true)
        {
            udpDTO.client.Send(udpDTO.data, udpDTO.length, udpDTO.endpoint);

            animsender.SetDetectionMode(true, udpDTO.endpoint.Address.ToString());

            yield return new WaitForSeconds(.1f);
        }
    }

    public void SendUDPMessage(UDPpacketDTO udpDTO)
    {
        udpDTO.client.Send(udpDTO.data, udpDTO.length, udpDTO.endpoint);
    }

    IEnumerator PollLamps(UDPpacketDTO udpDTO)
    {
        while (true)
        {
            byte[] Authentication = new byte[] { 0xD5, 0x0A, 0x80, 0x30 };

            //DebugText.text = string.Format("Polling lamps, {0} lamps found!", LampIPtoLengthDictionary.Count);
            udpDTO.client.Send(udpDTO.data, udpDTO.length, udpDTO.endpoint);
            Thread.Sleep(1000);
            IPEndPoint ReceivalEndpoint = new IPEndPoint(IPAddress.Any, 0);
            
            while (udpDTO.client.Available > 0)
            {
                var ReceivedMessageBytes = udpDTO.client.Receive(ref ReceivalEndpoint);

                //            //Parsing
                //            byte[] IPbytes = new byte[4];
                //            Array.Copy(ReceivedMessageBytes, 4, IPbytes, 0, 4);
                //            IPAddress LightIP = new IPAddress(IPbytes);
                //            int lightLength = ReceivedMessageBytes[25];//ReceivedMessageBytes[8] - 1;
                //            int batteryLevel = ReceivedMessageBytes [9];
                //byte[] macName = new byte[6];
                //Array.Copy(ReceivedMessageBytes, 10, macName, 0, 6);
                //string lampMacName = System.Text.Encoding.UTF8.GetString(macName);


                //Necessary information on lamps
                IPAddress LightIP = new IPAddress(0);
                int batteryLevel = 0;
                int numPixels = 0;
                string lampMacName = "";
                byte[] IDbytes = new byte[4];
                bool dontUseThisDevice = false;

                //Verification!
                Array.Copy(ReceivedMessageBytes, 0, IDbytes, 0, 4);

                if (ByteArrayCompare(IDbytes, Authentication))
                {
                    //Old byte ut
                    //Parsing
                    byte[] IPbytes = new byte[4];
                    Array.Copy(ReceivedMessageBytes, 4, IPbytes, 0, 4);
                    LightIP = new IPAddress(IPbytes);
                    //lightLength = ReceivedMessageBytes[8] - 1;
                    batteryLevel = ReceivedMessageBytes[9];
                    numPixels = ReceivedMessageBytes[25];
                    byte[] macName = new byte[6];
                    Array.Copy(ReceivedMessageBytes, 10, macName, 0, 6);
                    lampMacName = System.Text.Encoding.UTF8.GetString(macName);
                }
                else
                {
                    try
                    {
                        UDPResponse response = JsonConvert.DeserializeObject<UDPResponse>(Encoding.UTF8.GetString(ReceivedMessageBytes));
                        LightIP = new IPAddress(response.IP);
                        numPixels = response.length;
                        lampMacName = response.serial_name;
                        batteryLevel = response.battery_level;
                    }
                    catch (Exception)
                    {
                        dontUseThisDevice = true;
                    }
                }

                if (dontUseThisDevice)
                {
                    continue;
                }

                if (!IPtoProps.ContainsKey(LightIP))
                {
                    IPtoProps.Add(LightIP, new ExtraProperties
                    {
                        BatteryLevel = batteryLevel,
                        LampMac = lampMacName
                    });
                }



                if (!LampIPtoLengthDictionary.Keys.Contains(LightIP))
                {
                    LampIPtoLengthDictionary.Add(LightIP, numPixels);
                    NewLampIPtoLengthDictionary.Add(LightIP, numPixels);
                    GameObject.Find("DetectedLampProperties").GetComponent<DetectedLampProperties>().LampIPtoLengthDictionary.Add(LightIP, numPixels);
     //               GameObject.Find ("DetectedLampProperties").GetComponent<LampProperties>().batteryLevel = batteryLevel;
					//GameObject.Find ("DetectedLampProperties").GetComponent<LampProperties>().macName = lampMacName;
                }
            }

            yield return new WaitForSeconds(3f);
        }
    }

    private static byte[] SetLightValueArray(byte[] lightValues, byte[][] colorPattern, int pixelCount, int colorCount)
    {
        int lightStep = pixelCount / colorCount;
        for (int pix = 0; pix < pixelCount; pix++)
        {
            for (int c = 0; c < 4; c++)
            {
                int colorIndex = (pix / lightStep) == colorCount ? colorCount - 1 : pix / lightStep;
                lightValues[4 * pix + c] = colorPattern[colorIndex][c];
            }
        }
        return lightValues;
    }

    //NOTE: This MUCH simpler implementation of permutation generation had to be removed due to complications with Unity3D
    //private static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
    //{
    //    if (length == 1) return list.Select(t => new T[] { t }).ToArray();

    //    return GetPermutations(list, length - 1)
    //        .SelectMany((t) => list.Where(e => !t.Contains(e)),
    //            (t1, t2) => t1.Concat(new T[] { t2 }));
    //}

    //private static IEnumerable<IEnumerable<T>> GetIrreversiblePermutations<T>(List<T> list)
    //{
    //    return GetPermutations(list, list.Count())
    //        .Where(c => list.IndexOf(c.First()) < list.IndexOf(c.Last()))
    //        .Select(c => c);
    //}

    static bool ByteArrayCompare(byte[] a1, byte[] a2)
    {
        if (a1.Length != a2.Length)
            return false;

        for (int i = 0; i < a1.Length; i++)
            if (a1[i] != a2[i])
                return false;

        return true;
    }

    private static IEnumerable<List<T>> GetIrreversiblePermutations<T>(List<T> list)
    {
        return GeneratePermutations(list)
            .Where(c => list.IndexOf(c.First()) < list.IndexOf(c.Last()))
            .Select(c => c).ToList();
    }

    private static List<List<T>> GeneratePermutations<T>(List<T> items)
    {
        // Make an array to hold the
        // permutation we are building.
        T[] current_permutation = new T[items.Count];

        // Make an array to tell whether
        // an item is in the current selection.
        bool[] in_selection = new bool[items.Count];

        // Make a result list.
        List<List<T>> results = new List<List<T>>();

        // Build the combinations recursively.
        PermuteItems<T>(items, in_selection,
            current_permutation, results, 0);

        // Return the results.
        return results;
    }

    private static void PermuteItems<T>(List<T> items, bool[] in_selection,
    T[] current_permutation, List<List<T>> results,
    int next_position)
    {
        // See if all of the positions are filled.
        if (next_position == items.Count)
        {
            // All of the positioned are filled.
            // Save this permutation.
            results.Add(current_permutation.ToList());
        }
        else
        {
            // Try options for the next position.
            for (int i = 0; i < items.Count; i++)
            {
                if (!in_selection[i])
                {
                    // Add this item to the current permutation.
                    in_selection[i] = true;
                    current_permutation[next_position] = items[i];

                    // Recursively fill the remaining positions.
                    PermuteItems<T>(items, in_selection,
                        current_permutation, results,
                        next_position + 1);

                    // Remove the item from the current permutation.
                    in_selection[i] = false;
                }
            }
        }
    }

}

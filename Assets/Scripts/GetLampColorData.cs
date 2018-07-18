using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Text;
using System.Linq;
using System.Net;
using System.Net.Sockets;


public class GetLampColorData : MonoBehaviour {

	IPEndPoint lampEndPoint;
    //IPEndPoint sendingEnpoint;
	UdpClient receivingUDPClient;

    public SetupScripts setupScripts;
    public byte[] ReceivedMessageBytes;
	public Dictionary<string, byte[]> colorData = new Dictionary<string, byte[]>();
	public List<string> Mac;

	// Use this for initialization
	void Start () {
		Debug.Log ("GetLampColorData started....");

		lampEndPoint = new IPEndPoint (IPAddress.Any, 0);
        //sendingEnpoint = new IPEndPoint(IPAddress.Broadcast, 31000);

		if (receivingUDPClient == null) {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            var endPoint = new IPEndPoint(IPAddress.Any, 31000);
            socket.Bind(endPoint);

            receivingUDPClient = new UdpClient();
            receivingUDPClient.Client = socket;

            //receivingUDPClient = new UdpClient(31000); //animSender.LampCommunicationClient;
            //receivingUDPClient.Client.ReceiveBufferSize = 4096;
		}
        //receivingUDPClient.EnableBroadcast = true;
        //receivingUDPClient.Client.Blocking = false;
        //receivingUDPClient.Client.ReceiveTimeout = 40;
        //receivingUDPClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        receivingUDPClient.Client.ReceiveBufferSize = 1024;
        //Mac.Add("172.20.0.1");
		for (int i = 0; i < Mac.Count; i++) {
			//Debug.Log ("Sending data out to lamp....");
			receivingUDPClient.Send (new byte[] { 0 }, 1, new IPEndPoint(setupScripts.LampMactoIPDictionary[Mac[i]], 31000));
			//Debug.Log ("Data sent....");
		}
		//colorsArray = new byte[100, 4];

		StartCoroutine("GetColorData");

	}


	// receive color data from lamp
	IEnumerator GetColorData()
	{
		//Debug.Log ("Inside coroutine!");

		while (true)
		{
			if (receivingUDPClient.Available > 0) 
			{
                //Debug.Log("Available data: "+receivingUDPClient.Available);

				ReceivedMessageBytes = receivingUDPClient.Receive(ref lampEndPoint);

				if (ReceivedMessageBytes.Length == 0)
				{
					//myClient.Close();
					continue;
				}

                //Debug.Log ("Recevied data from lamp IP: "+lampEndPoint.Address.ToString());

                //Debug.Log ("Received Data = "+ReceivedMessageBytes.GetLength(0));
                string LampMac = setupScripts.LampMactoIPDictionary.FirstOrDefault(x => x.Value.ToString() == lampEndPoint.Address.ToString()).Key;

                if (LampMac == null)
                    continue;

                if (!colorData.ContainsKey (LampMac)) {
					colorData.Add (LampMac, ReceivedMessageBytes);
					//Debug.Log ("Added KEY to color dictionary");
				} else {
					colorData [LampMac] = ReceivedMessageBytes;
					//Debug.Log ("Added DATA to color dictionary");
				}

			} else {
                //Debug.Log("No data!");
				for (int i = 0; i < Mac.Count; i++) {
					receivingUDPClient.Send (new byte[] { 0 }, 1, new IPEndPoint(setupScripts.LampMactoIPDictionary[Mac[i]], 31000));
				}
                //receivingUDPClient.Send(new byte[] {0 }, 1, sendingEnpoint);
                yield return null;
            }

			//yield return new WaitForSeconds (0.01f);
			yield return null;
		}
	}




	// Update is called once per frame
	void Update () {
		
	}

    
}

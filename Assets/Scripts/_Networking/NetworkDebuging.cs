using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Voyager.Networking;

public class NetworkDebuging : MonoBehaviour {

	[SerializeField] Text networkPort31000Text;

	int counter;

	void Start()
	{
		NetworkManager.OnLampColorResponse += NetworkManager_OnLampColorResponse;
	}

	void NetworkManager_OnLampColorResponse(byte[] data, System.Net.IPAddress ip)
	{
		counter++;
		networkPort31000Text.text = "Color data received " + counter.ToString() + " times.";
    }
}
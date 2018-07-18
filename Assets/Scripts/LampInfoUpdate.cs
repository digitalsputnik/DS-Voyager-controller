using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System;

public class LampInfoUpdate : MonoBehaviour {

	Ribbon ribbon;
	[SerializeField] SetupScripts setup;
	public Text lampText;
	[SerializeField] float connectionLostTime;
	[SerializeField] GameObject warningImage;

	public string mac;
	//public IPAddress ip;
	ExtraProperties properties;
	public UDPResponse fullLastResponse;

	bool connectionLost;

    void Start()
    {
        ribbon = GetComponent<Ribbon>();
		mac = ribbon.Mac;
    }

	void Update()
	{
        if (!setup.MactoProps.ContainsKey(mac))
            return;

		if (setup.MactoProps[mac] != properties)
        {
			properties = setup.MactoProps[mac];
			connectionLost = false;
			//ip = setup.LampMactoIPDictionary[mac];
            ChangeText();
        }

		if (setup.LampsLastResponse[mac] != fullLastResponse)
			fullLastResponse = setup.LampsLastResponse[mac];

		if (!connectionLost && DateTime.Now.TimeOfDay.TotalSeconds >=
		    properties.LastUpdate.TimeOfDay.TotalSeconds + connectionLostTime)
		{
			connectionLost = true;
			ChangeText();
		}
	}

	void ChangeText()
	{
		string lampName = lampText.text.Split(' ')[0];
		lampText.text = lampName + " " + properties.LampMac + " " + properties.BatteryLevel + "% charged";
		if (connectionLost)
			lampText.text += " - connection lost";
		warningImage.SetActive(connectionLost);
	}
}

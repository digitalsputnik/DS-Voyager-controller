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
	[SerializeField] Image warningImage;

    public IPAddress ip;
	ExtraProperties properties;
	public UDPResponse fullLastResponse;

	bool connectionLost;

    void Start()
    {
        ribbon = GetComponent<Ribbon>();
        ip = IPAddress.Parse(ribbon.IP);
    }

	void Update()
	{
		if (setup.IPtoProps[ip] != properties)
        {
            properties = setup.IPtoProps[ip];
			connectionLost = false;
            ChangeText();
        }

		if (setup.LampsLastResponse[ip] != fullLastResponse)
			fullLastResponse = setup.LampsLastResponse[ip];

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
		warningImage.gameObject.SetActive(connectionLost);
	}
}

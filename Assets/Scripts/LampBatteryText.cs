using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net;

public class LampBatteryText : MonoBehaviour {

	Ribbon ribbon;
    SetupScripts setup;
    [SerializeField] Text lampInfo;

    IPAddress ip;
	int BatteryLevel = 0;

    void Start()
    {
        ribbon = GetComponent<Ribbon>();
        setup = GameObject.Find("SetupTools").GetComponent<SetupScripts>();
        ip = IPAddress.Parse(ribbon.IP);
    }

	void Update()
	{      
		if (!setup.IPtoProps.ContainsKey(ip))
			return;
                  
		if (BatteryLevel != setup.IPtoProps[ip].BatteryLevel)
		{
			BatteryLevel = setup.IPtoProps[ip].BatteryLevel;
			ChangeText();
		}
	}

	void ChangeText()
	{
		string lampName = lampInfo.text.Split(' ')[0];
		lampInfo.text = lampName + " " + ribbon.Mac + " " + BatteryLevel + "% charged";
	}
}

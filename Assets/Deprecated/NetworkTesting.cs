using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Voyager.Networking;
using Voyager.Lamps;
using Newtonsoft.Json;
using System.Net;

public class NetworkTesting : MonoBehaviour {

	LampManager lampManager;
	PhysicalLamp physicalLamp;

	void Start()
	{
		lampManager = GameObject.Find("Lamp Manager").GetComponent<LampManager>();
	}

	void Update ()
    {      
		if (Input.GetKeyDown(KeyCode.A))
		{
            Lamp lamp = lampManager.GetLamp(0);
			PhysicalLamp physicalLampNew = null;
            if (lamp != null)
				physicalLampNew = lampManager.InstantiateLamp(lamp);

			if (physicalLampNew != null)
				physicalLamp = physicalLampNew;
        }

		if (Input.GetKeyDown(KeyCode.Z))
        {
			if (physicalLamp != null)
				lampManager.DestroyLamp(physicalLamp);
        }

		if (Input.GetKeyDown(KeyCode.S))
		{
            Lamp lamp = lampManager.GetLamp(1);
            if (lamp != null)
				lampManager.InstantiateLamp(lamp);
        }

		if (Input.GetKeyDown(KeyCode.V))
		{
			Lamp lamp = lampManager.GetLamp(2);
			if (lamp != null)
				lampManager.InstantiateLamp(lamp);
        }
    }
}

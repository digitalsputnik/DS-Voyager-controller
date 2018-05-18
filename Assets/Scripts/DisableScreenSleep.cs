using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableScreenSleep : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
	}
	
}

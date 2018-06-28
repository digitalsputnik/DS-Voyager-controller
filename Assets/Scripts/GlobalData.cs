using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalData : MonoBehaviour {

	static GlobalData instance;

	List<string> notifiedHigherVersionLamps = new List<string>();
	public static List<string> NotifiedHigherVersionLamps
	{
		get { return instance.notifiedHigherVersionLamps; }
		set { instance.notifiedHigherVersionLamps = value; }
	}

	void Awake()
	{
		if(instance == null)
		{
			instance = this;
			return;
		}

		Destroy(gameObject);
	}
}
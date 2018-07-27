using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Voyager.Lamps;
using Voyager.Workspace;

public class AddLampBtn : MonoBehaviour {

	LampManager manager;
	Lamp lamp;

	public void Setup(Lamp lamp)
	{
		manager = GameObject.FindWithTag("LampManager").GetComponent<LampManager>();
		this.lamp = lamp;

		GetComponentInChildren<Text>().text = lamp.Serial;
	}

    public void Use()
	{
		Use(Vector3.zero);
	}

	public void Use(Vector3 position)
	{
		Workspace.InstantiateLamp(lamp, position);
		Transform parent = transform.parent;
		if (parent.childCount == 3)
			parent.GetChild(0).gameObject.SetActive(false);
		if (parent.childCount <= 2)
			parent.parent.parent.gameObject.SetActive(false);
        Destroy(gameObject);
	}
}
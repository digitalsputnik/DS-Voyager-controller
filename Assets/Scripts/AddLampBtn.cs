using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Voyager.Lamps;

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
		manager.InstantiateLamp(lamp, position);
		if (transform.parent.childCount == 3)
			transform.parent.GetChild(0).gameObject.SetActive(false);
		if (transform.parent.childCount <= 2)
			transform.parent.parent.parent.gameObject.SetActive(false);
        Destroy(gameObject);
	}
}
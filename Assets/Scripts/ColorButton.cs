using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorButton : MonoBehaviour {

	public GameObject cWheel;
	public DrawMode cLayer;

	// Use this for initialization
	void Start () {
		//cWheel.SetActive (true);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown (0)) {
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit, 100)) {
				if (hit.transform.name == "ColorB") {
					//display colorwheel
					cWheel.SetActive (true);
					//hide tool and color button
					GameObject.Find ("Tool").SetActive (false);
					this.gameObject.SetActive (false);

					//set itsh values from layer
					cWheel.GetComponent<ColorWheel>().setValues(cLayer.iVal,cLayer.tVal,cLayer.sVal,cLayer.hVal);
				}
			}
		}
	}
}

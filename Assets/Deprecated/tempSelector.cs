using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class tempSelector : MonoBehaviour {

	public Ribbon longRibbon;
	public Ribbon shortRibbon;

	Text label;

	// Use this for initialization
	void Start () {
		label = this.transform.GetChild (0).GetChild (0).GetComponent<Text> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (!Input.GetMouseButtonDown (0))
			return;

		Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast (ray, out hit, 100)) {
			if (hit.transform.name != "Selector")
				return;
			string current = label.text;
			if (current == "router") {
				label.text = "long";

				longRibbon.IP = "172.20.0.1";
				longRibbon.Restart ();

				shortRibbon.IP = "192.168.1.24";
				shortRibbon.Restart ();
			}
			if (current == "long") {
				label.text = "short";

				longRibbon.IP = "192.168.1.20";
				longRibbon.Restart ();

				shortRibbon.IP = "172.20.0.1";
				shortRibbon.Restart ();
			}
			if (current == "short") {
				label.text = "router";

				longRibbon.IP = "192.168.1.20";
				longRibbon.Restart ();

				shortRibbon.IP = "192.168.1.24";
				shortRibbon.Restart ();
			}

		
		
		}
	}
}

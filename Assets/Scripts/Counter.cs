using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class Counter : MonoBehaviour {

	public InputField InputValue;
	public int increment = 1;
	public Button PlusButton { get; set; }
	public Button MinusButton { get; set; }
	private int num = 0;

	// Use this for initialization
	void Start () {
		//Set input value
		InputValue.text = "1";

		//Get buttons
		PlusButton = transform.Find("PlusButton").GetComponent<Button>();
		MinusButton = transform.Find("MinusButton").GetComponent<Button>();

		//Add listeners to buttons
		PlusButton.onClick.AddListener(OnPlusClick);
		MinusButton.onClick.AddListener(OnMinusClick);
	}
		

	private void OnMinusClick()
	{

		if (int.TryParse(InputValue.text, out num)) {
			if (num == 0) {
				InputValue.text = num.ToString ();
			} else {
				InputValue.text = (num - increment).ToString ();
			}
		}
	}

	private void OnPlusClick()
	{
		if (int.TryParse(InputValue.text, out num)) {
			InputValue.text = (num + increment).ToString();
		}
	}
		

}
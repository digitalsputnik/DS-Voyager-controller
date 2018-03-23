using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler {

	public InputField InputValue;
	public int increment = 1;
	public Button button { get; set;}
	string buttonName;
	int num = 0;
	float lastTime = 0.1f;
	float timeDelay = 0.4f;
	bool isPressed;

	// Use this for initialization
	void Start () {
		//Set input value
		//InputValue.text = "1";

		//Get button
		button = gameObject.GetComponent<Button>();
			
		//Get button name
		buttonName = gameObject.name;

		//Add listeners to buttons
		if (buttonName == "MinusButton") {
			button.onClick.AddListener (OnMinusClick);
		}
		if (buttonName == "PlusButton") {
			button.onClick.AddListener (OnPlusClick);
		}

	}
		
	private void OnMinusClick()
	{

		if (int.TryParse(InputValue.text, out num)) {
			InputValue.text = (num - increment).ToString ();
		}
	}

	private void OnPlusClick()
	{
		if (int.TryParse(InputValue.text, out num)) {
			InputValue.text = (num + increment).ToString();
		}
	}
		


	public void OnPointerDown(PointerEventData eventData) {

		isPressed = true;
	}

	public void OnPointerUp(PointerEventData eventData) {

		isPressed = false;

	}

	public void OnPointerExit(PointerEventData eventData) {

		isPressed = false;

	}

	// Update is called once per frame
	void Update () {

		if (!isPressed)
			return;

		lastTime = lastTime + Time.deltaTime;
		if (lastTime <= timeDelay)
			return;

		if (buttonName == "MinusButton") {
			if (int.TryParse(InputValue.text, out num)) {
				if (num == 0) {
					InputValue.text = num.ToString ();
				} else {
					InputValue.text = (num - increment).ToString ();
				}
			}
		}

		if (buttonName == "PlusButton") {
			if (int.TryParse(InputValue.text, out num)) {
				InputValue.text = (num + increment).ToString();
			}
		}

		lastTime = 0.1f;


	}
}

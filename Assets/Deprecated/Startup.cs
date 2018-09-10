using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Startup : MonoBehaviour {

	public bool tutorialMode = false;
	public GameObject tutorial;
	public GameObject text1;
	public GameObject text2;
	public GameObject text3;
	public GameObject text4;
	public GameObject text5;
	public GameObject text6;
	public GameObject text7;
	public GameObject text8;
	public GameObject text9;
	public Button next;
	public Button cancel;

	public bool action1;
	public bool action2;
	public bool action3;

	private bool onMove = false;
	private Vector3 StartPosition;
	private Vector3 EndPosition;
	private float currentTime = 0.0f;
	private float normalizedTime = 0.0f;
	private float TimeOfTravel = 0.8f;
	private int pauseTime = 15;
	int i;


	// Use this for initialization
	void Start () {

		//fix screen to landscape mode
		Screen.autorotateToPortrait = false;
		Screen.autorotateToPortraitUpsideDown = false;

		//if (!PlayerPrefs.HasKey ("TutorialStatus"))
			PlayerPrefs.SetInt ("TutorialStatus", 0);

		if (PlayerPrefs.GetInt("TutorialStatus") == 0)
			StartTutorial();

		next.onClick.AddListener (OnNextButtonClick);
		cancel.onClick.AddListener (OnCancelButtonClick);

		Debug.Log ("Version "+Application.version);

	}

	void StartTutorial(){
		tutorialMode = true;
		EndPosition = text1.transform.position;
		EndPosition.y = EndPosition.y + 100;
		//Show Tutorial panel.
		tutorial.SetActive(true);

		//PlayerPrefs.SetInt ("TutorialStatus", 1);
	}

	void OnCancelButtonClick () {
		tutorialMode = false;
		tutorial.SetActive(false);
		//PlayerPrefs.SetInt ("TutorialStatus", 1);
	}

	void OnNextButtonClick () {
		if (text2.activeSelf) {
			text1.SetActive (false);
			text2.SetActive (false);
			text3.SetActive (true);
		}

		//PlayerPrefs.SetInt ("TutorialStatus", 1);
	}



	
	// Update is called once per frame
	void Update () {
		//Debug.Log ("Color.a == "+text1.GetComponent<Text> ().color.a.ToString());
		if (text1.GetComponent<Text> ().color.a >= 1.0f) {
			i += 1;
			if (i == pauseTime) {
				onMove = true;
			}
		}
		if (onMove)
		{
			currentTime += Time.deltaTime;
			normalizedTime = currentTime / TimeOfTravel;
			text1.transform.position = Vector3.Lerp(text1.transform.position, EndPosition, normalizedTime);
			if (currentTime >= TimeOfTravel)
			{
				onMove = false;
				text2.SetActive (true);
			}
		}

		if (action1) {
			text1.SetActive (false);
			text3.SetActive (false);
			text4.SetActive (true);
		}

		if (action2) {
			text4.SetActive (false);
			text5.SetActive (true);
		}

		if (action3) {
			text5.SetActive (false);
			text6.SetActive (true);
			text7.SetActive (true);
			text8.SetActive (true);
			text9.SetActive (true);
		}
	}
}

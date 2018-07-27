using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Voyager.Workspace;

public class MenuMode : MonoBehaviour {

    public GameObject drawMode;
    public GameObject drawTools;
    public GameObject setupMode;
    public GameObject workSpace;
	public Dropdown toolsDropDown;
    public Button DrawButton;
	public Button SetupButton;
	public Transform videoStream;
	bool toggle = false;
    //public GameObject colorButton2;

	void Start () {
		DrawButton.onClick.AddListener(TaskOnDrawClicked);
		SetupButton.onClick.AddListener(TaskOnSetupClicked);
	}

	public void TaskOnDrawClicked () {
		SetupButton.gameObject.SetActive (true);
		DrawButton.gameObject.SetActive (false);
		toggle = false;
		SetLightSetupStatus(toggle);
		//Startup start = GameObject.Find ("Main Camera").GetComponent<Startup> ();

		//if (start.tutorialMode){
		//	start.action3 = true;
		//}
	}

	void TaskOnSetupClicked () {
		DrawButton.gameObject.SetActive (true);
		SetupButton.gameObject.SetActive (false);
		toggle = true;
		SetLightSetupStatus(toggle);
	}

	private void SetLightSetupStatus(bool draw)
	{
		if (!draw) {
			toolsDropDown.value = 0;
		}

        drawMode.SetActive(!draw);
		drawTools.SetActive(!draw);
		setupMode.SetActive(draw);

        var lightsList = GameObject.FindGameObjectsWithTag("light");
        foreach (var light in lightsList)
		{
			light.transform.Find("DragAndDrop1").gameObject.SetActive(draw);
			light.transform.Find("DragAndDrop2").gameObject.SetActive(draw);
			light.transform.Find("Canvas").gameObject.SetActive(draw);
		}

		//if (videoStream.Find("Graphics").Find("Video Screen").gameObject.activeSelf)
   //     {
			//videoStream.transform.Find("DragHandle1").gameObject.SetActive(draw);
			//videoStream.transform.Find("DragHandle2").gameObject.SetActive(draw);
			//videoStream.transform.Find("Graphics").GetComponent<DragHandle>().enabled = draw;
        //}



    }

    public void SetupBtn()
	{
		Workspace.ShowGraphics();
	}

    public void DrawBtn()
	{
		Workspace.HideGraphics();
	}
}

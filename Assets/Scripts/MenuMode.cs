using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuMode : MonoBehaviour {

    public GameObject drawMode;
    public GameObject drawTools;
    public GameObject setupMode;
    public GameObject workSpace;
	public Dropdown toolsDropDown;
    public Button DrawButton;
	public Button SetupButton;
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
		Startup start = GameObject.Find ("Main Camera").GetComponent<Startup> ();

		if (start.tutorialMode){
			start.action3 = true;
		}
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
			light.transform.Find("Handle1").Find("DragAndDrop1").gameObject.SetActive(draw);
			light.transform.Find("Handle2").Find("DragAndDrop2").gameObject.SetActive(draw);
			light.transform.Find("Canvas").gameObject.SetActive(draw);
		}


        var videoStreamParent = GameObject.Find("VideoStreamParent");
        if (videoStreamParent.transform.Find("VideoStreamBackground").gameObject.activeSelf)
        {
            var videoStreamBackground = videoStreamParent.transform.Find("VideoStreamBackground");
            videoStreamBackground.transform.Find("Handle1Parent").Find("Handle1").gameObject.SetActive(draw);
            videoStreamBackground.transform.Find("Handle2Parent").Find("Handle2").gameObject.SetActive(draw);
        }



    }


	/*
    public static Toggle toggle;
    // Use this for initialization
	void Start () {
        toggle = this.GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(SwitchModes);
	}

    private void SwitchModes(bool arg0)
    {
        SetLightSetupStatus(toggle.isOn);
    }

    private void SetLightSetupStatus(bool activeVal)
    {
		if (!toggle.isOn) {
			toolsDropDown.value = 0;
		}
        drawMode.SetActive(!activeVal);
        drawTools.SetActive(!activeVal);
        //colorButton.SetActive(!activeVal);
        //colorButton2.SetActive(false);
        setupMode.SetActive(activeVal);
        
        int lightCount = workSpace.transform.childCount;
        for (int i = 0; i < lightCount; i++)
        {
            var light = workSpace.transform.GetChild(i);
            light.Find("DragAndDrop1").gameObject.SetActive(activeVal);
            light.Find("DragAndDrop2").gameObject.SetActive(activeVal);
            light.Find("Canvas").gameObject.SetActive(activeVal);
        }
    }
*/

}

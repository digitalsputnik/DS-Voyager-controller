using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LampSettings : MonoBehaviour {

    public Button ListenerToggleButton;
    public Button SaveButton;
    public AnimationSender animSender;
    public GameObject SetupTools;
    public GameObject LampStatusPanel;
    public Toggle MasterToggle;
    public Toggle ClientToggle;
    public GameObject SSID;
    public GameObject Password;

    string lampStatus = "";
    string SSIDValue = "";
    string PasswordValue = "";

    GameObject currentLamp;
    GameObject lastLamp;
    private bool selectLamp;
    List<GameObject> selectedLamps;

    // Use this for initialization
    void Start () {

        ListenerToggleButton.onClick.AddListener(TogglePacketListener);
        SaveButton.onClick.AddListener(SaveLampStatus);
        //MasterToggle.onValueChanged.AddListener(GetLampStatus);
        ClientToggle.onValueChanged.AddListener(GetClientProperties);

        selectedLamps = new List<GameObject>();
    }

    private void TogglePacketListener()
    {
        if (!animSender.ActiveStroke.layer.scene.ArtNetMode && !animSender.ActiveStroke.layer.scene.sACNMode)
        {
            //If both are switched off, turn on ArtNet
            animSender.ActiveStroke.layer.scene.ArtNetMode = true;
            animSender.ActiveStroke.layer.scene.sACNMode = false;
            ListenerToggleButton.GetComponentInChildren<Text>().text = "ArtNet: On";

        }
        else if (animSender.ActiveStroke.layer.scene.ArtNetMode)
        {
            //If ArtNet is on, turn on sACN
            animSender.ActiveStroke.layer.scene.ArtNetMode = false;
            animSender.ActiveStroke.layer.scene.sACNMode = true;
            ListenerToggleButton.GetComponentInChildren<Text>().text = "sACN: On";
        }
        else
        {
            //Turn both off
            animSender.ActiveStroke.layer.scene.ArtNetMode = false;
            animSender.ActiveStroke.layer.scene.sACNMode = false;
            ListenerToggleButton.GetComponentInChildren<Text>().text = "ArtNet/sACN: Off";
        }
        animSender.SendAnimationWithUpdate();
    }

    public void OnBackButtonClick()
    {
        var lightsList = GameObject.FindGameObjectsWithTag("light");
        foreach (var light in lightsList)
        {
            light.transform.Find("DragAndDrop1").gameObject.SetActive(true);
            light.transform.Find("DragAndDrop2").gameObject.SetActive(true);
            //light.transform.Find("Canvas").gameObject.SetActive(false);
        }

        var videoStreamParent = GameObject.Find("VideoStreamParent");
        if (videoStreamParent.transform.Find("VideoStreamBackground").gameObject.activeSelf)
        {
            var videoStreamBackground = videoStreamParent.transform.Find("VideoStreamBackground");
            videoStreamBackground.transform.Find("Handle1Parent").Find("Handle1").gameObject.SetActive(true);
            videoStreamBackground.transform.Find("Handle2Parent").Find("Handle2").gameObject.SetActive(true);
        }

        SetupTools.SetActive(true);

        this.gameObject.SetActive(false);
    }

    void GetClientProperties(bool value)
    {
        if (MasterToggle.isOn)
        {
            lampStatus = "master";
        }
        else
        if (ClientToggle.isOn)
        {
            SSID.SetActive(true);
            Password.SetActive(true);
        }
        else {
            SSID.SetActive(false);
            Password.SetActive(false);
        }
        Debug.Log("Lamp status is: " + lampStatus);
    }

    void SaveLampStatus()
    {
        SSIDValue = SSID.transform.Find("InputField").GetComponent<InputField>().text;
        PasswordValue = Password.transform.Find("InputField").GetComponent<InputField>().text;

        if (MasterToggle.isOn)
        {
            lampStatus = "master";
        }
        else if (ClientToggle.isOn)
        {
            lampStatus = "client";
        }

        if (selectedLamps.Count > 0)
        {
            foreach (var lamp in selectedLamps)
            {
                lamp.GetComponent<Ribbon>().status = lampStatus;
                lamp.GetComponent<Ribbon>().ssid = SSIDValue;
                lamp.GetComponent<Ribbon>().password = PasswordValue;
            }
        }
    }

        

 
    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonUp(0))
        {
            currentLamp = null;
            foreach (var lamp in selectedLamps)
            {
                Debug.Log("Lamps in List: "+ lamp.name);
            }

        }

        if (Input.touchCount == 2)
        {
            return;

        }
        else
        if (Input.GetMouseButtonDown(0))
        {
            //if clicked on lamp, select it
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100) && hit.transform.tag == "lamp")
            {
                
                currentLamp = hit.transform.parent.parent.gameObject;
                Debug.Log("Selected lamp = " + currentLamp.name);
                if (selectedLamps.Contains(currentLamp))
                {
                    selectedLamps.Remove(currentLamp);
                    currentLamp.transform.Find("Canvas").Find("Text").GetComponent<Text>().color = Color.white;
                    if (selectedLamps.Count == 0)
                    {
                        ListenerToggleButton.gameObject.SetActive(false);
                        MasterToggle.gameObject.SetActive(false);
                        ClientToggle.gameObject.SetActive(false);
                        SSID.SetActive(false);
                        Password.SetActive(false);
                        SaveButton.gameObject.SetActive(false);
                        LampStatusPanel.transform.Find("HeadingText").gameObject.SetActive(false);
                        LampStatusPanel.transform.Find("MessageText").gameObject.SetActive(true);
                    }
                }
                else
                {
                    selectedLamps.Add(currentLamp);
                    currentLamp.transform.Find("Canvas").Find("Text").GetComponent<Text>().color = Color.red;
                    if (LampStatusPanel.transform.Find("MessageText").gameObject.activeSelf)
                    {
                        //Remove Message Text
                        LampStatusPanel.transform.Find("MessageText").gameObject.SetActive(false);
                        //Enable Lamp properties
                        ListenerToggleButton.gameObject.SetActive(true);
                        LampStatusPanel.transform.Find("HeadingText").gameObject.SetActive(true);
                        MasterToggle.gameObject.SetActive(true);
                        ClientToggle.gameObject.SetActive(true);
                        SaveButton.gameObject.SetActive(true);
                    }
                }
                 
            }

        }
    }
}

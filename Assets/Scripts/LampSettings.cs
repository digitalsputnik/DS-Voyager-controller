using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Collections;
using System.Linq;
using UnityEngine.EventSystems;
using Voyager.Lamps;
using Voyager.Networking;

public class LampSettings : MonoBehaviour {

	[Header("UI Settings")]
	[SerializeField] Color lampSelectedColor;
	[SerializeField] Color lampNormalColor;
    [Header("UI Elements")]
	public Text selectionStateText;

	[Space(3)]
	public GameObject clientModeBtn;
	public GameObject clientModePanel;
	public Toggle     clientModeFieldToggle;
	public InputField clientModeFieldSsid;
	public Toggle     clientModeDropToggle;
	public Dropdown   clientModeDropSsid;
	public GameObject clientModeDropLoading;
	public InputField clientModePassword;

	[Space(3)]
	public GameObject apModeBtn;
	public GameObject apModePanel;
	public GameObject apModePanelMultiple;
	public InputField apModeSsid;
	public InputField apModePassword;
    [Header("When Leaving The Menu")]
	[SerializeField] GameObject[] setActiveWhenLeaveMenu;

	[SerializeField] List<PhysicalLamp> selectedLamps = new List<PhysicalLamp>();
	Dictionary<PhysicalLamp, List<string>> LampToSSIDlistDictionary = new Dictionary<PhysicalLamp, List<string>>();

	bool entered;

	void Update()
	{
		if(!entered)
		{
			entered = true;
			foreach (PhysicalLamp lamp in selectedLamps)
				lamp.Text.color = lampSelectedColor;

			foreach (GameObject lamp in GameObject.FindGameObjectsWithTag("light"))
				lamp.transform.Find("Canvas").GetComponent<GraphicRaycaster>().enabled = false;

			UpdateUI();
		}

		CheckForLamps();
	}

    public void ExitMenu()
	{
		foreach (GameObject gObject in setActiveWhenLeaveMenu)
			gObject.SetActive(true);

		foreach (PhysicalLamp lamp in selectedLamps)
			lamp.Text.color = lampNormalColor;

		foreach (GameObject lamp in GameObject.FindGameObjectsWithTag("light"))
            lamp.transform.Find("Canvas").GetComponent<GraphicRaycaster>().enabled = true;

		entered = false;
		gameObject.SetActive(false);
	}

	void CheckForLamps()
	{
		if (EventSystem.current.IsPointerOverGameObject())
            return;

		if (Input.touchCount > 0)
			if (IsTouchOverUIObject(Input.GetTouch(0)))
				return;

		if (Input.GetMouseButtonUp(0))
        {
            foreach (var lamp in selectedLamps)
				Debug.Log("Lamps in List: " + lamp.Owner.Serial);
        }

        if (Input.touchCount == 2)
			return;
		
        if (Input.GetMouseButtonDown(0))
        {
            //if clicked on lamp, select it
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 100))
            {
				Debug.Log(hit.transform.name);
				if (hit.transform.name == "Graphics")
				{
					PhysicalLamp currentLamp = hit.transform.parent.GetComponent<PhysicalLamp>();

					if (selectedLamps.Contains(currentLamp))
						LampDeselected(currentLamp);
					else
						LampSelected(currentLamp);
				}
            }
        }
	}

	void LampSelected(PhysicalLamp lamp)
	{
		selectedLamps.Add(lamp);
		lamp.Text.color = lampSelectedColor;
		NetworkManager.AskLampSsidList(lamp.Owner.IP);
		UpdateUI();
	}

	void LampDeselected(PhysicalLamp lamp)
	{
		lamp.Text.color = lampNormalColor;
		selectedLamps.Remove(lamp);
		UpdateUI();
	}

    void UpdateUI()
	{
		SelectionState selectionState = GetSelectionState();
		selectionStateText.text = GetSelectionStateMessage(selectionState);

		clientModePanel.SetActive(false);
		apModePanel.SetActive(false);
		apModePanelMultiple.SetActive(false);

		if(selectionState == SelectionState.None)
		{
			clientModeBtn.SetActive(false);
			apModeBtn.SetActive(false);
		}
		else
		{
			clientModeBtn.SetActive(true);
            apModeBtn.SetActive(true);
		}
	}

	SelectionState GetSelectionState()
	{
		if (selectedLamps.Count == 0)
			return SelectionState.None;
        
		List<Lamp> lampsWithApMode = new List<Lamp>();
		List<Lamp> lampsWithClientMode = new List<Lamp>();

		foreach (PhysicalLamp lamp in selectedLamps)
        {
			if (lamp.Owner.Mode == "ap_mode")
				lampsWithApMode.Add(lamp.Owner);
			else if (lamp.Owner.Mode == "client_mode")
				lampsWithClientMode.Add(lamp.Owner);
            else
				Debug.LogError("Lamp " + lamp.Owner.Serial + " doesn't have a active_mode data.");
        }

		if (lampsWithApMode.Count == selectedLamps.Count)
			return SelectionState.AdModes;

		if (lampsWithClientMode.Count == selectedLamps.Count)
			return SelectionState.ClientModes;
		
		return SelectionState.Mixed;
	}

	string GetSelectionStateMessage(SelectionState state)
	{      
		if (selectedLamps.Count == 0)
			return "Select at last one lamp";
		else
		{
			if (state == SelectionState.ClientModes)
			{
				if (selectedLamps.Count == 1)
					return "The lamp is connected to other";
				else
					return "The lamps are connected to somewhere else";
			}
			else if (state == SelectionState.AdModes)
			{
				if (selectedLamps.Count == 1)
					return "The lamp is currently an access point";
				else
					return "The lamps are currently an access points";
			}
			else if (state == SelectionState.None)
				return "There is no mode on this lamp, I quess the lamp needs some updates";
            else
                return "There's a mix of modes on selected lamps";
        }
	}
    
    public void TurnToSlave()
	{
		bool opening = !clientModePanel.activeSelf;
		Lamp lastSelectedLamp = selectedLamps[selectedLamps.Count - 1].Owner;

		if (opening)
		{
			clientModeFieldSsid.text = lastSelectedLamp.activePattern;
			clientModePassword.text = lastSelectedLamp.activePatternPassword;

			clientModeFieldToggle.isOn = true;
			clientModeDropToggle.isOn = false;

            clientModeDropSsid.ClearOptions();
            clientModeDropSsid.AddOptions(CreateSsidList());

			if(clientModeDropSsid.options.Count == 0)
			{
				clientModeDropSsid.interactable = false;
				clientModeDropToggle.interactable = false;
				clientModeDropLoading.SetActive(true);
			}

			foreach (PhysicalLamp pLamp in selectedLamps)
				StartCoroutine(GetSSIDListForLamp(pLamp));

            clientModePanel.SetActive(true);

			apModeBtn.SetActive(false);
		}
		else
		{
			clientModePanel.SetActive(false);
			apModeBtn.SetActive(true);
        }
	}

    public void Connect()
	{
		//StartCoroutine(SendClientModeToSelectedLamps());
		string ssid = "";

        if (clientModeFieldToggle.isOn)
            ssid = clientModeFieldSsid.text;
        else
            ssid = clientModeDropSsid.options[clientModeDropSsid.value].text;
		
		foreach(PhysicalLamp lamp in selectedLamps)
		{
			NetworkManager.AskTurnClientMode(lamp.Owner.IP, ssid, clientModePassword.text);
        }

		SendingDataStarted();
	}

    public void MakeAccessPoint()
	{      
		if(selectedLamps.Count > 1)
		{
			apModeSsid.text = "";
			apModePassword.text = "";

			bool opening = !apModePanelMultiple.activeSelf;

            if (opening)
            {
				apModePanelMultiple.SetActive(true);
                clientModeBtn.SetActive(false);
                clientModePanel.SetActive(false);
            }
            else
            {
				apModePanelMultiple.SetActive(false);
                clientModeBtn.SetActive(true);
            }
		}
        else
		{
            bool opening = !apModePanel.activeSelf;

            if (opening)
            {
				Lamp lamp = selectedLamps[0].Owner;
				apModeSsid.text = lamp.activeSSID;
				apModePassword.text = lamp.activePassword;
				apModePanel.SetActive(true);

				clientModeBtn.SetActive(false);
				clientModePanel.SetActive(false);
            }
            else
            {
				apModePanel.SetActive(false);
				clientModeBtn.SetActive(true);
            }
		}
	}

    public void Make()
	{
		//StartCoroutine(SendApModeToSelectedLamps());
		foreach(PhysicalLamp lamp in selectedLamps)
		{
			int channel = lamp.Owner.activeChannel;
			string ssid = (apModeSsid.text == "") ? lamp.Owner.activeSSID : apModeSsid.text;
			string password = (apModePassword.text == "") ? lamp.Owner.activePassword : apModePassword.text;
			NetworkManager.AskTurnApMode(lamp.Owner.IP, channel, ssid, password);
		}

        SendingDataStarted();
	}

	public List<string> CreateSsidList()
	{
		List<string> ssids = new List<string>();

        var listOfSSIDLists = new List<List<string>>();
        foreach (var lamp in selectedLamps)
        {
            if (LampToSSIDlistDictionary.ContainsKey(lamp))
            {
                listOfSSIDLists.Add(LampToSSIDlistDictionary[lamp]);
            }
            else
            {
                return ssids;
            }
        }

        var intersection = listOfSSIDLists.Skip(1).Aggregate(new HashSet<string>(listOfSSIDLists.First()), (h, e) => { h.IntersectWith(e); return h; });
        ssids = intersection.ToList();

        return ssids;
	}

	IEnumerator GetSSIDListForLamp(PhysicalLamp lamp)
    {
        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        int port = 30000;
        
        byte[] ReceiveBuffer = new byte[2048];
        bool SSIDnotReceived = true;

        while (SSIDnotReceived)
        {
			sock.SendTo(new byte[] { 0xD5, 0x0A, 0x87, 0x10 }, new IPEndPoint(lamp.Owner.IP, port));
            yield return new WaitForSeconds(1.0f);
            while (sock.Available > 0)
            {
                int BufferSize = sock.Receive(ReceiveBuffer);
                byte[] ReceivedBytes = ReceiveBuffer.Take(BufferSize).ToArray();
                List<string> ssidsForLamp = JsonConvert.DeserializeObject<List<string>>(Encoding.UTF8.GetString(ReceivedBytes));
                if (ssidsForLamp != null)
                {
                    if (LampToSSIDlistDictionary.ContainsKey(lamp))
                    {
                        LampToSSIDlistDictionary[lamp] = ssidsForLamp;
                    }
                    else
                    {
                        LampToSSIDlistDictionary.Add(lamp, ssidsForLamp);
                    }
                    SSIDnotReceived = false;
                }
                else
                {
                    Debug.Log("Another message");
                }
            }
            yield return null;
        }
		clientModeDropSsid.ClearOptions();
		clientModeDropSsid.AddOptions(CreateSsidList());

        clientModeDropSsid.interactable = true;
        clientModeDropToggle.interactable = true;
        clientModeDropLoading.SetActive(false);

        yield return null;
    }

	//byte[] CreateClientModeJsonPackage()
 //   {
 //       string ssid = "";
    //
 //       if (clientModeFieldToggle.isOn)
 //           ssid = clientModeFieldSsid.text;
 //       else
 //           ssid = clientModeDropSsid.options[clientModeDropSsid.value].text; // clientModeDropSsid.itemText.text;
    //
	//	ClientModePackage package = new ClientModePackage()
	//	{
	//		set_pattern = ssid,
 //           set_pattern_ps = clientModePassword.text
 //       };
 //       return Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(package));
 //   }
    //
	//IEnumerator SendClientModeToSelectedLamps()
	//{
	//	SendingDataStarted();
	//	Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
	//	int port = 30000;
	//	List<IPEndPoint> endPoints = new List<IPEndPoint>();
	//	byte[] package = CreateClientModeJsonPackage();
    //
	//	foreach (LampInfoUpdate info in lampInfo.Values)
	//		endPoints.Add(new IPEndPoint(setupScripts.LampMactoIPDictionary[info.mac], port));
    //
 //       for (int i = 0; i < 10; i++)
 //       {
 //           foreach (IPEndPoint endPoint in endPoints)
 //               sock.SendTo(package, endPoint);
 //           yield return new WaitForSeconds(0.2f);
 //       }
    //
 //       SendingDataEnded();
	//	yield return null;
	//}

    void SendingDataStarted()
	{
		clientModePanel.SetActive(false);
		clientModeBtn.SetActive(true);

		apModePanel.SetActive(false);
		apModeBtn.SetActive(true);
	}

    void SendingDataEnded()
	{
		UpdateUI();
	}

	bool IsTouchOverUIObject(Touch touch)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = touch.position;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}

public class ApModePackage
{
	public string network_mode = "ap_mode";
	public int set_channel { get; set; }
	public string set_ssid { get; set; }
	public string set_password { get; set; }
}

public class ClientModePackage
{
	public string network_mode = "client_mode";
	public string set_pattern { get; set; }
	public string set_pattern_ps { get; set; }
}

public enum SelectionState
{
	AdModes, ClientModes, Mixed, None
}
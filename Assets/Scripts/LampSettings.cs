using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Collections;
using System.Linq;

public class LampSettings : MonoBehaviour {

	[Header("UI Settings")]
	[SerializeField] Color lampSelectedColor;
	[SerializeField] Color lampNormalColor;
    [Header("UI Elements")]
	[SerializeField] Text selectionStateText;

	[Space(3)]
	[SerializeField] GameObject clientModeBtn;
	[SerializeField] GameObject clientModePanel;
	[SerializeField] Toggle     clientModeFieldToggle;
	[SerializeField] InputField clientModeFieldSsid;
	[SerializeField] Toggle     clientModeDropToggle;
	[SerializeField] Dropdown   clientModeDropSsid;
	[SerializeField] InputField clientModePassword;

	[Space(3)]
	[SerializeField] GameObject apModeBtn;
	[SerializeField] GameObject apModePanel;
	[SerializeField] InputField apModeSsid;
	[SerializeField] InputField apModePassword;
    [Header("When Leaving The Menu")]
	[SerializeField] GameObject[] setActiveWhenLeaveMenu;

	List<GameObject> selectedLamps = new List<GameObject>();
	Dictionary<GameObject, LampInfoUpdate> lampInfo = new Dictionary<GameObject, LampInfoUpdate>();
    Dictionary<GameObject, List<string>> LampToSSIDlistDictionary = new Dictionary<GameObject, List<string>>();

	bool entered;

	void Update()
	{
		if(!entered)
		{
			entered = true;
			foreach (LampInfoUpdate lamp in lampInfo.Values)
				lamp.lampText.color = lampSelectedColor;
			UpdateUI();
		}

		CheckForLamps();
	}

    public void ExitMenu()
	{
		foreach (GameObject gObject in setActiveWhenLeaveMenu)
			gObject.SetActive(true);

		foreach (LampInfoUpdate lamp in lampInfo.Values)
			lamp.lampText.color = lampNormalColor;

		entered = false;
		gameObject.SetActive(false);
	}

	void CheckForLamps()
	{
		if (Input.GetMouseButtonUp(0))
        {
            foreach (var lamp in selectedLamps)
				Debug.Log("Lamps in List: " + lamp.name);
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
				if (hit.transform.tag == "lamp")
				{
					GameObject currentLamp = hit.transform.parent.parent.gameObject;

					if (selectedLamps.Contains(currentLamp))
						LampDeselected(currentLamp);
					else
						LampSelected(currentLamp);
				}
            }
        }
	}

	void LampSelected(GameObject lamp)
	{
		selectedLamps.Add(lamp);
		lampInfo.Add(lamp, lamp.GetComponent<LampInfoUpdate>());

		lampInfo[lamp].lampText.color = lampSelectedColor;
		UpdateUI();
	}

	void LampDeselected(GameObject lamp)
	{
		lampInfo[lamp].lampText.color = lampNormalColor;

		selectedLamps.Remove(lamp);
		lampInfo.Remove(lamp);

		UpdateUI();
	}

    void UpdateUI()
	{
		SelectionState selectionState = GetSelectionState();
		selectionStateText.text = GetSelectionStateMessage(selectionState);

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
		if (lampInfo.Count == 0)
			return SelectionState.None;

		List<LampInfoUpdate> lampsWithApMode = new List<LampInfoUpdate>();
        List<LampInfoUpdate> lampsWithClientMode = new List<LampInfoUpdate>();

        foreach (LampInfoUpdate info in lampInfo.Values)
        {
            if (info.fullLastResponse.active_mode == "ap_mode")
                lampsWithApMode.Add(info);
            else if (info.fullLastResponse.active_mode == "client_mode")
                lampsWithClientMode.Add(info);
            else
                Debug.LogError("Lamp " + info.fullLastResponse.serial_name + " doesn't have a active_mode data.");
        }

		if (lampsWithApMode.Count == lampInfo.Count)
			return SelectionState.AdModes;
		else if (lampsWithClientMode.Count == lampInfo.Count)
			return SelectionState.ClientModes;
		else
			return SelectionState.Mixed;
	}

	string GetSelectionStateMessage(SelectionState state)
	{      
		if (lampInfo.Count == 0)
			return "Select at last one lamp";
		else
		{
			if (state == SelectionState.ClientModes)
			{
				if (lampInfo.Count == 1)
					return "The lamp is connected to other";
				else
					return "The lamps are connected to somewhere else";
			}
			else if (state == SelectionState.AdModes)
			{
				if (lampInfo.Count == 1)
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
		LampInfoUpdate lastSelectedLamp = lampInfo[selectedLamps[selectedLamps.Count - 1]];

		if (opening)
		{
			clientModeFieldSsid.text = lastSelectedLamp.fullLastResponse.active_pattern;
			clientModePassword.text = lastSelectedLamp.fullLastResponse.active_pattern_ps;

			clientModeFieldToggle.isOn = true;
			clientModeDropToggle.isOn = false;

			clientModeDropSsid.ClearOptions();
			clientModeDropSsid.AddOptions(CreateSsidList());

			clientModePanel.SetActive(true);

			apModeBtn.SetActive(false);
			apModePanel.SetActive(false);
		}
		else
		{
			clientModePanel.SetActive(false);
			apModeBtn.SetActive(true);
        }
	}

    public void Connect()
	{
		StartCoroutine(SendClientModeToSelectedLamps());
	}

    public void MakeAccessPoint()
	{      
		if(selectedLamps.Count > 1)
		{
			apModeSsid.text = "";
			apModePassword.text = "";
			StartCoroutine(SendApModeToSelectedLamps());
		}
        else
		{
            bool opening = !apModePanel.activeSelf;

            if (opening)
            {
				LampInfoUpdate lamp = lampInfo[selectedLamps[0]];
				apModeSsid.text = lamp.fullLastResponse.active_ssid;
				apModePassword.text = lamp.fullLastResponse.active_password;
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
		StartCoroutine(SendApModeToSelectedLamps());
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
    
	byte[] CreateApModeJsonPackage(LampInfoUpdate lamp)
	{
		ApModePackage package = new ApModePackage()
		{
			set_channel = lamp.fullLastResponse.active_channel,
			set_ssid = (apModeSsid.text == "") ? lamp.fullLastResponse.active_ssid : apModeSsid.text,
			set_password = (apModePassword.text == "") ? lamp.fullLastResponse.active_password : apModePassword.text
		};

		return Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(package));
	}
    
	IEnumerator SendApModeToSelectedLamps()
    {
        SendingDataStarted();
        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        int port = 30000;
        Dictionary<IPEndPoint, byte[]> endPoints = new Dictionary<IPEndPoint, byte[]>();

        foreach (LampInfoUpdate info in lampInfo.Values)
            endPoints.Add(new IPEndPoint(info.ip, port), CreateApModeJsonPackage(info));

        for (int i = 0; i < 10; i++)
        {
            foreach (IPEndPoint endPoint in endPoints.Keys)
                sock.SendTo(endPoints[endPoint], endPoint);
            yield return new WaitForSeconds(0.2f);
        }

        SendingDataEnded();
        yield return null;
    }

	byte[] CreateClientModeJsonPackage()
    {
        string ssid = "";

		if (clientModeFieldToggle.isOn)
			ssid = clientModeFieldSsid.text;
		else
			ssid = clientModeDropSsid.itemText.text;

		ClientModePackage package = new ClientModePackage()
		{
			set_pattern = ssid,
            set_pattern_ps = clientModePassword.text
        };
        return Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(package));
    }

	IEnumerator SendClientModeToSelectedLamps()
	{
		SendingDataStarted();
		Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		int port = 30000;
		List<IPEndPoint> endPoints = new List<IPEndPoint>();
		byte[] package = CreateClientModeJsonPackage();

		foreach (LampInfoUpdate info in lampInfo.Values)
			endPoints.Add(new IPEndPoint(info.ip, port));

		for (int i = 0; i < 10; i++)
		{
			foreach(IPEndPoint endPoint in endPoints)
				sock.SendTo(package, endPoint);
			yield return new WaitForSeconds(0.2f);
		}
        
		SendingDataEnded();
		yield return null;
	}

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

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Voyager.Lamps;

public class SetupTools : MonoBehaviour {

	LampManager lampManager;

	[Header("UI")]
	[SerializeField] GameObject addLampsWindow;
	[SerializeField] GameObject addLampBtnPrefab;
	[SerializeField] GameObject addLampsText;
	[SerializeField] Transform addLampBtnParent;
	[SerializeField] GameObject addAllLampsBtn;
	[Space(5)]
    [SerializeField] Transform aboutWindow;
	[Header("Updating")]
	public float updateCheckInterval = 5.0f;
	public Vector2Int animVersion;
	public Vector2Int lpcVersion2ft;
	public Vector2Int lpcVersion4ft;
	public Vector2Int chipVersion;

	bool oneLampChecked;

	void Start()
	{
		lampManager = GameObject.FindWithTag("LampManager").GetComponent<LampManager>();

		if (lampManager == null)
		{
			Debug.LogError("Lamp Manager was not found.");
			gameObject.SetActive(false);
			return;
		}

		addAllLampsBtn.GetComponent<Button>().onClick.AddListener(AddAllLampsBtnClick);
		InvokeRepeating("CheckUpdates", 1.0f, updateCheckInterval);
	}

	void Update()
    {
        if (Input.touchCount == 0 && !Input.GetMouseButton(0))
        {
			CameraMove cameraMove = Camera.main.GetComponent<CameraMove>();
			if (cameraMove.enabled == false)
				cameraMove.enabled = true;
        }

		if (lampManager.GetLamps().Count == 1 && !oneLampChecked)
		{
			Invoke("OneLampAdd", 1.5f);
			oneLampChecked = true;
		}
    }

    public void CheckUpdates()
	{
		List<Lamp> uncheckedLamps = lampManager.GetUncheckedLamps();
        
		if(uncheckedLamps.Count > 0)
		{
			List<Lamp> lampsToUpdate = new List<Lamp>();

			foreach(Lamp lamp in uncheckedLamps)
			{
				bool update = false;
				Vector2Int lpcVersion = (lamp.Lenght > 50) ? lpcVersion4ft : lpcVersion2ft;

				if (lamp.animationVersion[0] < animVersion.x)
					update = true;
				else if (lamp.animationVersion[0] == animVersion.x &&
						 lamp.animationVersion[1] < animVersion.y)
					update = true;

				if (lamp.lpcVersion[0] < lpcVersion.x)
					update = true;
				else if (lamp.lpcVersion[0] == lpcVersion.x &&
						 lamp.lpcVersion[1] < lpcVersion.y)
					update = true;

				if (update)
					lampsToUpdate.Add(lamp);
				else
					lamp.updateChecked = true;
			}

			if (lampsToUpdate.Count > 0)
			{
				List<string> updateIPs = new List<string>();
				foreach(Lamp lamp in lampsToUpdate)
				{
					updateIPs.Add(lamp.IP.ToString());
                }
				GameObject.FindWithTag("Networking").GetComponent<LampUpdater>().UpdateLampsSoftware(updateIPs);
			}
		}
	}

	void OneLampAdd()
    {
		if (lampManager.GetAddableLamps().Count == 1)
			lampManager.InstantiateLamp(lampManager.GetLamp(0));
    }

	public void DetectLamps()
	{
		// Some woodoo magic here.
		// Ideas:
		//  - Save the workspace.
        //  - Go to another scene to detect.
	}

    public void AddLamps()
	{
		foreach (Transform go in addLampBtnParent)
        {
			if (go.name != "Add All Lamps")
				Destroy(go.gameObject);
        }

		List<Lamp> lamps = lampManager.GetAddableLamps();

		addLampsText.SetActive(lamps.Count == 0);
		addAllLampsBtn.SetActive(lamps.Count > 1);

		foreach(Lamp lamp in lamps)
		{
			GameObject newBtn = Instantiate(addLampBtnPrefab, addLampBtnParent);
			newBtn.GetComponent<AddLampBtn>().Setup(lamp);
		}
		addLampsWindow.SetActive(true);
	}

	void AddAllLampsBtnClick()
	{
		int count = 0;
		List<AddLampBtn> lampsToAdd = new List<AddLampBtn>();

		foreach (Transform go in addLampBtnParent)
        {
            if (go.name != "Add All Lamps")
            {
				lampsToAdd.Add(go.GetComponent<AddLampBtn>());
				count++;
            }
        }

		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2.0f, Screen.height));

		Physics.Raycast(ray, out hit);
		Vector3 startPos = hit.point;

		ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2.0f, 0.0f));

		Physics.Raycast(ray, out hit);
		Vector3 endPos = hit.point;

		float fullDistance = startPos.y - endPos.y;
		float distance = fullDistance / (count + 1);
		float currentPos = startPos.y - distance;

		for (int i = 0; i < count; i++)
		{
			lampsToAdd[i].Use(new Vector3(startPos.x, currentPos));
			currentPos -= distance;
		}

		addLampsWindow.SetActive(false);
	}

    public void Save()
	{
		lampManager.SaveWorkplace();
	}

    public void Load()
	{
		foreach (Lamp lamp in lampManager.GetLampsInWorkplace())
			lampManager.DestroyLamp(lamp.physicalLamp);
		lampManager.LoadWorkplace();
	}

    public void Exit()
	{
		Application.Quit();
	}

    public void About()
	{
		Text versionText = aboutWindow.Find("Version Text").GetComponent<Text>();
        versionText.text = "Version " + Application.version;
		aboutWindow.gameObject.SetActive(true);
	}
}
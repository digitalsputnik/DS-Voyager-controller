using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Voyager.Lamps;
using Voyager.Workspace;
using System;
using Voyager.Networking;
using Crosstales.FB;
using NatCamU.Core;

public class SetupTools : MonoBehaviour {

	LampManager lampManager;

	[Header("UI")]
	[SerializeField] GameObject addLampsWindow;
	[SerializeField] GameObject addLampBtnPrefab;
	[SerializeField] GameObject addLampsText;
	[SerializeField] Transform addLampBtnParent;
	[SerializeField] GameObject addAllLampsBtn;
	[SerializeField] SavePanel savePanel;
	[SerializeField] LoadPanel loadPanel;
	[Space(5)]
    [SerializeField] Transform aboutWindow;
	[Header("Updating")]
	public float updateCheckInterval = 5.0f;
	public Vector2Int animVersion;
	public Version hw3Version;
	public Version hw4Version;

    bool oneLampChecked;

	void Start()
	{
		lampManager = LampManager.Instance;

		if (lampManager == null)
		{
			Debug.LogError("Lamp Manager was not found.");
			gameObject.SetActive(false);
			return;
		}

		addAllLampsBtn.GetComponent<Button>().onClick.AddListener(AddAllLampsBtnClick);

		if (PlayerPrefs.GetInt("ComingFromDetectionScene") != 0)
			ComingFromDetection();
                  
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

    void ComingFromDetection()
	{
		Workspace.LoadWorkplace("main_temp");

		if (PlayerPrefs.GetInt("ComingFromDetectionScene") == 2)
			Workspace.LoadWorkplace("detection");
        PlayerPrefs.SetInt("ComingFromDetectionScene", 0);

        File.Delete(Application.persistentDataPath + "/workspaces/main_temp.dsw");
        File.Delete(Application.persistentDataPath + "/workspaces/detection.dsw");

		Invoke("SetDetectionModeFalse", 2.0f);
	}

    void SetDetectionModeFalse()
	{
		foreach(Lamp lamp in lampManager.GetLamps())
			NetworkManager.SetDetectionMode(lamp.IP, false);
	}

    public void CheckUpdates()
	{
		List<Lamp> uncheckedLamps = lampManager.GetUncheckedLamps();
        
		if(uncheckedLamps.Count > 0)
		{
			List<Lamp> lampsToUpdate = new List<Lamp>();

			foreach(Lamp lamp in uncheckedLamps)
			{
				if(lamp.animationVersion != null)
				{
					Version version = lamp.hardwareVersion == 4 ? hw4Version : hw3Version;
					bool update = false;
					
					if (lamp.animationVersion[0] < animVersion.x)
						update = true;
					else if (lamp.animationVersion[0] == animVersion.x && lamp.animationVersion[1] < animVersion.y)
						update = true;
					
					if (lamp.lpcVersion[0] < version.lpcVersion.x)
						update = true;
					else if (lamp.lpcVersion[0] == version.lpcVersion.x && lamp.lpcVersion[1] < version.lpcVersion.y)
						update = true;

					if (lamp.chipVersion[0] < version.chipVersion.x)
                        update = true;
					else if (lamp.chipVersion[0] == version.chipVersion.x && lamp.chipVersion[1] < version.chipVersion.y)
                        update = true;

					if (update)
						lampsToUpdate.Add(lamp);
					else
						lamp.upToDate = true;

					lamp.updateChecked = true;
                }
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
			Workspace.InstantiateLamp(lampManager.GetLamp(0));
    }

	public void DetectLamps()
	{
		NatCam.Release();
		Workspace.SaveWorkplace("main_temp");
		SceneManager.LoadScene(1);
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
			GameObject newBtn = Instantiate(addLampBtnPrefab, addLampBtnParent.transform);
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
		savePanel.Open();
	}   

    public void Load()
	{
		loadPanel.Open();
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

	public void OpenPhoto()
	{
		string documentsPath = "";

        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
			documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        else
			documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

		if (!Application.isMobilePlatform)
		{
			string file = FileBrowser.OpenSingleFile("Open Picture", documentsPath, new ExtensionFilter[] { new ExtensionFilter("Media", "png") });
            if(file != "")
			    LoadFileUsingPath(file);
        }
		else
		{
			NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
            {
				Debug.Log(path);
                if (path != "")
					LoadFileUsingPath(path);
            }, "Select a PNG image", "image/png");
		}

			//mobileImagePicker.Show("Select Image", DateTime.Now.ToShortDateString().Replace("/", "-") + "_" + DateTime.Now.ToShortTimeString().Replace(":", "-"), 2048);
	}

	void LoadFileUsingPath(string obj)
	{
		byte[] file = File.ReadAllBytes(obj);
		Texture2D texture = new Texture2D(2, 2);
		texture.LoadImage(file);
		string photoName = Path.GetFileName(obj).Split('.')[0];
		if (photoName == "tmp")
			photoName = Guid.NewGuid().ToString();
		string extenstion = Path.GetFileName(obj).Split('.')[Path.GetFileName(obj).Split('.').Length - 1];

		switch (extenstion)
		{
			case "png":
			case "jpg":
				Workspace.InstantiateImage(texture, photoName);
                break;
        }
	}
}

[Serializable]
public class Version
{
	public Vector2Int lpcVersion;
	public Vector2Int chipVersion;
}
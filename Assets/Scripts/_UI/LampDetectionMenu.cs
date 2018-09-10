using UnityEngine;
using UnityEngine.SceneManagement;
using Voyager.Lamps;
using Voyager.Workspace;
using UnityEngine.UI;
using System;
using NatCamU.Core;
using System.Collections;

public class LampDetectionMenu : MonoBehaviour {
    
    public void BackBtn()
	{
		PlayerPrefs.SetInt("ComingFromDetectionScene", 1);
		SceneManager.LoadScene(0);
		NatCam.Release();
	}
    
    public void AddDetectedLampsBtn()
	{      
		PlayerPrefs.SetInt("ComingFromDetectionScene", 2);
		Workspace.SaveWorkplace("detection");
		SceneManager.LoadScene(0);
		NatCam.Release();
	}

    public void AddDetectedLampsWithPicture()
	{
		StartCoroutine(AddDetectedLampsWithPictureEnumerator());
	}

	IEnumerator AddDetectedLampsWithPictureEnumerator()
	{
		NatCam.Camera.ExposureMode = ExposureMode.AutoExpose;

		yield return new WaitForSeconds(1.0f);

		Texture currentTexture = GetComponentInChildren<RawImage>().texture;
        Texture2D texture2D = new Texture2D(currentTexture.width, currentTexture.height);
        OpenCVForUnity.Utils.textureToTexture2D(currentTexture, texture2D);

        Photo imageObject = Workspace.InstantiateImage(texture2D, DateTime.Now.ToShortDateString().Replace("/", "-") + "_" + DateTime.Now.ToShortTimeString().Replace(":", "-"));
		imageObject.photoName = Guid.NewGuid().ToString();
        WorkspaceItem wItem = imageObject.GetComponent<WorkspaceItem>();
        LampMove move = imageObject.GetComponent<LampMove>();

        float halfPoint = GetComponent<LampDetectionCam>().worldPointWidth / 2.0f;
        Vector3 point1 = new Vector3(-halfPoint, 0.0f, 0.0f);
        Vector3 point2 = new Vector3(halfPoint, 0.0f, 0.0f);

        move.SetPosition(point1, point2);

        LampManager lampManager = LampManager.Instance;
        foreach (Lamp lamp in lampManager.GetLampsInWorkplace())
            lamp.physicalLamp.GetComponent<WorkspaceItem>().SetParent(wItem);

        PlayerPrefs.SetInt("ComingFromDetectionScene", 2);
        Workspace.SaveWorkplace("detection");
        SceneManager.LoadScene(0);
        NatCam.Release();
	}
}
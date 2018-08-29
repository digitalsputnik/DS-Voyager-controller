using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Voyager.Workspace;

public class SavePanel : MonoBehaviour {

	CanvasGroup canvasGroup;
	public InputField nameField;
	public PhotoCamera cam;

	void Start()
	{
		canvasGroup = GetComponent<CanvasGroup>();
	}
    public void Save()
	{
		Workspace.SaveWorkplace(nameField.text, cam.WorkspacePhoto());
		Close();
	}

	public void Open()
	{
		if (nameField.text == "")
			nameField.text = DateTime.Now.ToShortDateString().Replace("/", "-") + "_" + DateTime.Now.ToShortTimeString().Replace(":", "-");
		canvasGroup.alpha = 1.0f;
		canvasGroup.interactable = true;
		canvasGroup.blocksRaycasts = true;
	}

    public void Close()
	{
		canvasGroup.alpha = 0.0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
	}

    public void Cancel()
	{
		Close();
	}
}
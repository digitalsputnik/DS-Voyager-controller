using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LoadPanel : MonoBehaviour {

	CanvasGroup canvasGroup;
	[SerializeField] Transform content;
	[SerializeField] GameObject template;
	public SavePanel savePanel;
   
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Open()
    {
        canvasGroup.alpha = 1.0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

		foreach(Transform t in content)
			Destroy(t.gameObject);

		string[] paths = Directory.GetFiles(Application.persistentDataPath + "/workspaces");
		foreach(string path in paths)
		{
			if(path.EndsWith(".dsw", new System.StringComparison()) == true)
			{
				try { Instantiate(template, content).GetComponent<LoadItem>().Setup(path, this); }
				catch (Exception ex) { Debug.LogError(ex.Message); }
                
            }
		}
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateProgressText : MonoBehaviour {

	Text updateText;
	[SerializeField] LampUpdater lampUpdater;

	void Start()
	{
		updateText = GetComponent<Text>();
		updateText.enabled = false;
	}

	void Update()
	{
		if (lampUpdater.Updating && !updateText.enabled)
			updateText.enabled = true;
        
		if(updateText.enabled)
		{
			float progress = 0;
			foreach (float lampProgress in lampUpdater.UpdateProgress.Values)
				progress += lampProgress;
			int lampCount = lampUpdater.UpdatesInProgress;
            
			float overallProgress = progress * 100f / lampCount;

			string lamp = (lampCount == 1) ? "lamp" : "lamps";
			updateText.text = "Updating " + lampCount + " " + lamp + ". " + (int)overallProgress + "%";
		}

		if (!lampUpdater.Updating && updateText.enabled)
            updateText.enabled = false;
	}
}
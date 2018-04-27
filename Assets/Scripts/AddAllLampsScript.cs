using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddAllLampsScript : MonoBehaviour {

    public int ButtonCount;

    // Use this for initialization
	void Start () {
        this.gameObject.GetComponent<Button>().onClick.AddListener(AddAllLamps);
	}

    public void AddAllLamps()
    {
        ButtonCount = this.transform.parent.childCount;
        for (int childIndex = 0; childIndex < ButtonCount; childIndex++)
        {
            var Button = this.transform.parent.GetChild(childIndex);
            if (Button.name == "LampOptionButtonLong" || Button.name == "LampOptionButtonShort")
                continue;

            if (Button.gameObject.activeSelf)
            {
                var addScript = Button.GetComponent<AddLampButtonScript>();
                if (addScript != null)
                    addScript.TaskOnClickOverride(childIndex-3);
            }
        }
        this.gameObject.SetActive(false);
        this.transform.parent.parent.gameObject.SetActive(false);
    }
}

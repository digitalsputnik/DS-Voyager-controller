using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddAllLampsScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
        this.gameObject.GetComponent<Button>().onClick.AddListener(AddAllLamps);
	}

    public void AddAllLamps()
    {
        int addButtonCount = this.transform.parent.childCount;
        for (int childIndex = 0; childIndex < addButtonCount; childIndex++)
        {
            var Button = this.transform.parent.GetChild(childIndex);
            if (Button.name == "LampOptionButtonLong" || Button.name == "LampOptionButtonShort")
                continue;

            if (Button.gameObject.activeSelf)
            {
                var addScript = Button.GetComponent<AddLampButtonScript>();
                if (addScript != null)
                    addScript.TaskOnClickOverride();
            }
        }
        this.gameObject.SetActive(false);
        this.transform.parent.parent.gameObject.SetActive(false);
    }
}

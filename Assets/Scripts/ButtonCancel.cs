using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonCancel : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        var cancelButton = gameObject.GetComponent<Button>();
        cancelButton.onClick.AddListener(TaskHideParent);

    }

    private void TaskHideParent() {
        transform.parent.gameObject.SetActive(false);
    }



}

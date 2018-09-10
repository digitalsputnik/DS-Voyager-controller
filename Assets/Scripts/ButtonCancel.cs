using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonCancel : MonoBehaviour {

    void TaskHideParent() {
        transform.parent.gameObject.SetActive(false);
    }   
}

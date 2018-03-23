using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotificationDialogController : MonoBehaviour {

    private void Update()
    {
        HideIfClickedOutside(this.gameObject);
    }

    private void HideIfClickedOutside(GameObject panel)
    {
        //Camera camera = panel.GetComponent<Canvas>().renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main;
        if (Input.GetMouseButton(0) && panel.activeSelf &&
            !RectTransformUtility.RectangleContainsScreenPoint(
                panel.GetComponent<RectTransform>(),
                Input.mousePosition,
                null))
        {
            panel.SetActive(false);
        }
    }
}

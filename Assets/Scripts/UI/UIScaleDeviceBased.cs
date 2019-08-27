using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScaleDeviceBased : MonoBehaviour
{
    float curDPI;

    public GameObject Canvas;

    //Detects device size at the start
    private void Awake()
    {

        // If the device size is less than 7 inches set this as the reference resolution
        if (GetScreenSize() < 7)
        {
            Canvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        }

        // If the device size is more than 7 inches
        else if (GetScreenSize() >= 7)
        {
            Canvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(2560, 1600);
        }
    }

    public float GetScreenSize()
    {
        curDPI = Screen.dpi;
        return ((Mathf.Sqrt(Screen.width * Screen.width + Screen.height * Screen.height)) / curDPI);
    }
}

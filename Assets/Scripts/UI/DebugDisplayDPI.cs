using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugDisplayDPI : MonoBehaviour
{
    float curDPI;
    float screenHeight;
    float screenWidth;
    float referenceHeight;
    float referenceWidth;

    public GameObject Canvas;
    public GameObject[] UITEXT;

    // Start is called before the first frame update
    void Start()
    {
        curDPI = Screen.dpi;
    }

    // Update is called once per frame
    void Update()
    {
        referenceWidth = Canvas.GetComponent<CanvasScaler>().referenceResolution.x;
        referenceHeight = Canvas.GetComponent<CanvasScaler>().referenceResolution.y;
        screenHeight = Screen.height;
        screenWidth = Screen.width;

        UITEXT[0].GetComponent<Text>().text = "DPI: " + curDPI;
        UITEXT[1].GetComponent<Text>().text = "ScreenSizeInInches: " + GetScreenSize();
        UITEXT[2].GetComponent<Text>().text = "ScreenW: " + screenWidth + " ScreenH: " + screenHeight;
        UITEXT[3].GetComponent<Text>().text = "RefW: " + referenceWidth + " RefH: " + referenceHeight;
        UITEXT[4].GetComponent<Text>().text = "RefSizeInInches: " + getRefScreenSize();

        GetScreenSize();
        getRefScreenSize();
    }

    public float GetScreenSize()
    {
        return ((Mathf.Sqrt(Screen.width * Screen.width + Screen.height * Screen.height)) / curDPI);
    }

    public float getRefScreenSize()
    {
        return ((Mathf.Sqrt(referenceWidth * referenceWidth + referenceHeight * referenceHeight)) / curDPI);
    }

}

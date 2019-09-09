using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderToggle : MonoBehaviour
{
    public RectTransform ColorWheelMenu;
    public RectTransform myrect;

    public VerticalLayoutGroup VerticalLayoutGroup;

    float rectWidth;
    float menuWidth;


    public bool startUpdate;

    private void Start()
    {
        rectWidth = myrect.GetComponent<RectTransform>().rect.width; //Canvas length
        menuWidth = ColorWheelMenu.GetComponent<RectTransform>().rect.xMax;
    }

    public void ExpandMenu()
    {
        //Magic number 40 to fit the screen. Scales with different resolutions.
        ColorWheelMenu.offsetMax = new Vector2(menuWidth + rectWidth + 40, 0);
    }

    public void SliderModeEnabled()
    {
        setVLG(80, 80, -80, 0, 48);
    }

    public void SliderModeDisabled()
    {
        setVLG(4,4,32,0,70);
    }

    public void setVLG(int left, int right, int top, int bottom, int spacing)
    {
        VerticalLayoutGroup.padding.left = left;
        VerticalLayoutGroup.padding.right = right;
        VerticalLayoutGroup.padding.top = top;
        VerticalLayoutGroup.padding.bottom = bottom;
        VerticalLayoutGroup.spacing = spacing;
    }


    public void BackToNormal()
    {
        ColorWheelMenu.offsetMax = new Vector2(0, 0);
    }


}

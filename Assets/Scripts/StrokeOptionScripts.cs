using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StrokeOptionScripts : MonoBehaviour {

    public tempAnimcontroller Animation;
    public Text ActiveStrokeText;
    public DrawMode draw;
    public int AnimationdropDownValue;
    public AnimationControllerScripts AnimController;
    
    //public void OnSelectButtonClick()
    //{
    //    //No need to select, if this is active
    //    if (draw.ActiveStroke != this.gameObject)
    //        SelectCurrentAnimation();

    //    //Close window!
    //    this.transform.parent.parent.gameObject.SetActive(false);
    //}

    //public void SelectCurrentAnimation()
    //{
    //    //Save current animation
    //    var activeAnimation = draw.ActiveStroke.GetComponent<StrokeOptionScripts>().Animation;
    //    var TopPixels = activeAnimation.layer.controlledPixelsOnTop;
    //    Destroy(draw.ActiveStroke.GetComponent<StrokeOptionScripts>().Animation.gameObject);
    //    draw.ActiveStroke.GetComponent<StrokeOptionScripts>().Animation = Instantiate(draw.anim);
    //    draw.ActiveStroke.GetComponent<StrokeOptionScripts>().Animation.transform.name = this.GetComponentInChildren<InputField>().text; //Keep it's name
    //    draw.ActiveStroke.GetComponent<StrokeOptionScripts>().Animation.layer.controlledPixelsOnTop = TopPixels;

    //    //TODO: Set colors! for colorwheel

    //    //Select this animation
    //    draw.anim = Animation;
    //    draw.ActiveStroke = this.gameObject;
    //    AnimController.anim = draw.anim;
    //    ActiveStrokeText.text = this.GetComponentInChildren<InputField>().text;
    //}

    //public void OnDeleteButtonClick()
    //{
    //    //Check if it is active!
    //    if (this.gameObject == draw.ActiveStroke)
    //    {
    //        //Check if this is the latest stroke
    //        if (this.transform.parent.GetChild(this.transform.parent.childCount - 1) == this.transform)
    //        {
    //            //Check if it is the last one!
    //            if (this.transform.parent.childCount == 2)
    //            {
    //                //Clear current animation and create new!
    //                draw.anim.layer.controlledPixels.Clear();
    //                draw.OnNewStrokeAddClick();
    //            }
    //            else
    //            {
    //                //Select previous stroke
    //                this.transform.parent.GetChild(this.transform.parent.childCount - 2).GetComponent<StrokeOptionScripts>().SelectCurrentAnimation();
    //            }
    //        }
    //        else
    //        {
    //            //Select latest stroke
    //            this.transform.parent.GetChild(this.transform.parent.childCount - 1).GetComponent<StrokeOptionScripts>().SelectCurrentAnimation();
    //        }
    //    }

    //    //TODO: relocate previous layers (pixels on top)
    //    //TODO: if no Pixels are occupied, paint it black!

    //    //NOTE: On some occasions, it is possible to delete animation all together
    //    //Destroy animation and this selection
    //    Destroy(Animation.gameObject);
    //    Destroy(this.gameObject);
    //}
}

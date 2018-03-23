using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Net;

public class DragAndDropHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    public GameObject TrashCan;
    public GameObject setupMode;
    public SetupScripts setupScripts;
	public GameObject menuCanvas;
    public AnimationSender animSender;

    public static GameObject itemBeingDragged;
    Vector3 difference;
	Rect binRect;
	Image binImage;

    public void OnBeginDrag(PointerEventData eventData)
    {
        //if (!setupMode.activeSelf)
          //  return;

        TrashCan.SetActive(true);
        //TODO: Check if setup mode is on!
        //TODO: Check if pixel or handle is dragged and act accordingly.
        //NOTE: For this case, 
        itemBeingDragged = gameObject;
        difference = transform.position - GetMouseLampPosition();
    }

    public void OnDrag(PointerEventData eventData)
    {
        //if (!setupMode.activeSelf)
          //  return;
        transform.position = GetMouseLampPosition() + difference;

		//fix by Tahir
		binImage = TrashCan.GetComponent<Image>();
		binRect = new Rect(new Vector2(binImage.rectTransform.position.x,binImage.rectTransform.position.y-(binImage.rectTransform.sizeDelta.y*menuCanvas.GetComponent<RectTransform>().localScale.y)/2), new Vector2((binImage.rectTransform.sizeDelta.x * menuCanvas.GetComponent<RectTransform>().localScale.x),(binImage.rectTransform.sizeDelta.y * menuCanvas.GetComponent<RectTransform>().localScale.y)) ); 
		//new Rect(new Vector2(TrashCan.transform.position.x,TrashCan.transform.position.y-(TrashCan.GetComponent<RectTransform>().sizeDelta.y/2)) , TrashCan.GetComponent<RectTransform>().rect.size);
		//Debug.Log("Canvas scale is: "+ menuCanvas.GetComponent<RectTransform>().localScale);
		//Debug.Log ("Rect position, size: "+binRect.position.ToString()+", "+binRect.size.ToString());
		//Debug.Log ("Image position, size: "+binImage.transform.position.ToString()+", "+binImage.rectTransform.rect.width+","+binImage.rectTransform.rect.height);


		if (binRect.Contains (Input.mousePosition)) {
			binImage.color = Color.red;
		} else {
			binImage.color = Color.white;
		}

    }

    public void OnEndDrag(PointerEventData eventData)
    {
       // if (!setupMode.activeSelf)
         //   return;
        itemBeingDragged = null;
        transform.position = GetMouseLampPosition() + difference;
        //if dropped on Bin, get rid of the lamp! 
        /*Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, 100);
        if (hit.transform.name == "RemoveLamp")
        {
            Destroy(gameObject);
        }*/

		//fix by Tahir
		if(binRect.Contains(Input.mousePosition)){
			binImage.color = Color.white;
			Destroy (this.gameObject);
            IPAddress thisIP = IPAddress.Parse(this.GetComponent<Ribbon>().IP);
            if (setupScripts.LampIPtoLengthDictionary.ContainsKey(thisIP))
            {
                setupScripts.LampIPtoLengthDictionary.Remove(thisIP);
            }

            //Remove controlled pixels from strokes and animation
            animSender.RemoveLampFromStrokes(this.transform, thisIP.ToString());
		}

        TrashCan.SetActive(false);
    }

    private Vector3 GetMouseLampPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
		Physics.Raycast (ray, out hit, 100);
		return new Vector3 (hit.point.x, hit.point.y, 1);

    }


}

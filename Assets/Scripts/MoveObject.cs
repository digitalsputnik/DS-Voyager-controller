using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Net;

public class MoveObject : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    public DetectedLampProperties detectedLampProperties { get; set; }
    public GameObject TrashCan;
    public GameObject setupMode;
    public SetupScripts setupScripts;
    public GameObject menuCanvas;
    public AnimationSender animSender;
    public bool dragged = false;
    public bool endDrag = false;

    public static GameObject itemBeingDragged;
    Vector3 difference;
    Rect binRect;
    Image binImage;

 
    public void OnBeginDrag(PointerEventData eventData)
    {
        //if (!setupMode.activeSelf)
        //  return;

        if (dragged)
            return;

        if (gameObject.transform.tag == "set")
        {
            if (gameObject.transform.Find("VideoStreamBackground").Find("Handle1Parent").Find("Handle1").GetComponent<ScaleRotateObject>().handleDragged == true)
            {
                return;
            }
        }


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

        if (dragged)
        {
            return;
        }

        if (gameObject.transform.tag == "set")
        {
            if (gameObject.transform.Find("VideoStreamBackground").Find("Handle1Parent").Find("Handle1").GetComponent<ScaleRotateObject>().handleDragged == true)
            {
                return;
            }
        }

        transform.position = GetMouseLampPosition() + difference;

        //fix by Tahir
        binImage = TrashCan.GetComponent<Image>();
        binRect = new Rect(new Vector2(binImage.rectTransform.position.x, binImage.rectTransform.position.y - (binImage.rectTransform.sizeDelta.y * menuCanvas.GetComponent<RectTransform>().localScale.y) / 2), new Vector2((binImage.rectTransform.sizeDelta.x * menuCanvas.GetComponent<RectTransform>().localScale.x), (binImage.rectTransform.sizeDelta.y * menuCanvas.GetComponent<RectTransform>().localScale.y)));
        //new Rect(new Vector2(TrashCan.transform.position.x,TrashCan.transform.position.y-(TrashCan.GetComponent<RectTransform>().sizeDelta.y/2)) , TrashCan.GetComponent<RectTransform>().rect.size);
        //Debug.Log("Canvas scale is: "+ menuCanvas.GetComponent<RectTransform>().localScale);
        //Debug.Log ("Rect position, size: "+binRect.position.ToString()+", "+binRect.size.ToString());
        //Debug.Log ("Image position, size: "+binImage.transform.position.ToString()+", "+binImage.rectTransform.rect.width+","+binImage.rectTransform.rect.height);


        if (binRect.Contains(Input.mousePosition))
        {
            binImage.color = Color.red;
        }
        else
        {
            binImage.color = Color.white;
        }

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("Set position is: " + this.gameObject.transform.position);
        Debug.Log("Set rotation is: " + this.gameObject.transform.rotation.eulerAngles);

        if (dragged || endDrag)
        {
            return;
        }

        if (gameObject.transform.tag == "set")
        {
            if (gameObject.transform.Find("VideoStreamBackground").Find("Handle1Parent").Find("Handle1").GetComponent<ScaleRotateObject>().handleDragged == true)
            {
                return;
            }
        }



        itemBeingDragged = null;
        transform.position = GetMouseLampPosition() + difference;


        if (binRect.Contains(Input.mousePosition))
        {
            binImage.color = Color.white;
            Destroy(this.gameObject);
 
        }

        TrashCan.SetActive(false);
    }

    private Vector3 GetMouseLampPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, 100);
        return new Vector3(hit.point.x, hit.point.y, 1);

    }




}

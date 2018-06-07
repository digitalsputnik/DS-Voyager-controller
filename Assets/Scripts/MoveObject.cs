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
        if (!setupMode.activeSelf)
          return;

        if (dragged)
            return;

        if (gameObject.transform.tag == "videobg")
        {
            if (gameObject.transform.Find("VideoStreamBackground").Find("Handle1Parent").Find("Handle1").GetComponent<ScaleRotateObject>().handleDragged == true)
            {
                return;
            }
        }


 
        //TODO: Check if setup mode is on!
        //TODO: Check if pixel or handle is dragged and act accordingly.
        //NOTE: For this case, 
        itemBeingDragged = gameObject;
        difference = transform.position - GetMouseLampPosition();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!setupMode.activeSelf)
          return;

        if (dragged)
        {
            return;
        }

        if (gameObject.transform.tag == "videobg")
        {
            if (gameObject.transform.Find("VideoStreamBackground").Find("Handle1Parent").Find("Handle1").GetComponent<ScaleRotateObject>().handleDragged == true)
            {
                return;
            }
        }

        transform.position = GetMouseLampPosition() + difference;

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //Debug.Log("Set position is: " + this.gameObject.transform.position);
        //Debug.Log("Set rotation is: " + this.gameObject.transform.rotation.eulerAngles);

        if (!setupMode.activeSelf)
            return;

        if (dragged || endDrag)
        {
            return;
        }

        if (gameObject.transform.tag == "videobg")
        {
            if (gameObject.transform.Find("VideoStreamBackground").Find("Handle1Parent").Find("Handle1").GetComponent<ScaleRotateObject>().handleDragged == true)
            {
                return;
            }
        }



        itemBeingDragged = null;
        transform.position = GetMouseLampPosition() + difference;

    }

    private Vector3 GetMouseLampPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, 100);
        return new Vector3(hit.point.x, hit.point.y, 1);

    }




}

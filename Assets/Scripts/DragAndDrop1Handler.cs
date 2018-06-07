using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragAndDrop1Handler : MonoBehaviour {

    public GameObject referenceObject;
    public static GameObject itemBeingDragged;
    public static GameObject itemBeingScaled;
    public bool handleDragged = false;
    //public GameObject videoStreamBackground;

    //New
    Vector3 MouseDifference;
    Quaternion initialLightRotation;

    Vector3 initialDirection;
    Vector3 initialLightRelativePosition;
    Vector3 initialLightAbsolutePosition;
    Vector3 initialScale;
    Vector3 HandlerScale;

    private void OnMouseDown()
    {
       /* if (videoStreamBackground.activeSelf)
        {
            videoStreamBackground.transform.parent.GetComponent<MoveObject>().dragged = true;
        }*/
        itemBeingDragged = gameObject; //Drag and drop handle
        if (transform.parent.parent.gameObject.name == "Set")
        {
            itemBeingScaled = transform.parent.parent.parent.gameObject; //Set0
            handleDragged = true;
            Debug.Log("handleDragged = TRUE");

            var setsList = GameObject.FindGameObjectsWithTag("set");
            if (setsList.Length > 0)
            {
                for (int i = 0; i < setsList.Length; i++)
                {
                    var setObject = setsList[i];
                    Debug.Log("Set Name is: "+setObject.name);
                    setObject.GetComponent<DragAndDropHandler>().dragged = true;
                    setObject.GetComponent<DragAndDropHandler>().endDrag = false;
                }
            }
            else {
                Debug.Log("No set found!!!");
            }

            //var setId = Convert.ToInt32(itemBeingScaled.name.Substring(3, 1)) - 1;
            //GameObject.Find("Set"+setId).GetComponent<DragAndDropHandler>().dragged = true;

        }
        else
        {
            itemBeingScaled = transform.parent.parent.gameObject; //Light
            itemBeingScaled.transform.parent.parent.gameObject.GetComponent<DragAndDropHandler>().dragged = true;
                                                                  

        }



         //referenceObject = transform.parent.Find("DragAndDrop2").gameObject; //Other drag and drop handle
        //TODO: Get all information for calculation
        MouseDifference = GetMouseLampPosition() - itemBeingDragged.transform.position;
        initialLightRotation = itemBeingScaled.transform.rotation;

        initialDirection = itemBeingDragged.transform.position - referenceObject.transform.position;
        initialLightRelativePosition = itemBeingScaled.transform.position - referenceObject.transform.position;
        initialLightAbsolutePosition = itemBeingScaled.transform.position;
        initialScale = itemBeingScaled.transform.localScale;
        HandlerScale = this.transform.localScale;

        //Make trashcan appear!
        //TrashCan.SetActive(true);
    }

    private void OnMouseDrag()
    {
        
        //Rotation
        var newDirection = GetMouseLampPosition() - MouseDifference - referenceObject.transform.position;
        var newRotation = Quaternion.FromToRotation(initialDirection, newDirection);
        var newLightRotation = Quaternion.Euler(newRotation.eulerAngles + initialLightRotation.eulerAngles);
        itemBeingScaled.transform.rotation = newLightRotation;
        //Set rotation value in DragAndDropHandler
        itemBeingScaled.GetComponent<DragAndDropHandler>().rotation = newLightRotation.eulerAngles;

        //Position
        if (transform.parent.parent.tag == "light")
        {
            var T = (newDirection - initialDirection) / initialDirection.magnitude * initialLightRelativePosition.magnitude;
            var newLightPosition = initialLightAbsolutePosition + T;
            itemBeingScaled.transform.position = newLightPosition;
            //Set position value in DragAndDropHandler
            itemBeingScaled.GetComponent<DragAndDropHandler>().position = newLightPosition;
        }

        //Scale
        var newScale = initialScale;
        if (transform.parent.parent.tag == "light")
        {
            newScale = initialScale / initialDirection.magnitude * newDirection.magnitude;
        }
        else
        {
            var scaleX = initialScale.x / initialDirection.magnitude * newDirection.magnitude;
            var scaleY = initialScale.y / initialDirection.magnitude * newDirection.magnitude;
            var scaleZ = initialScale.z;
            newScale = new Vector3(scaleX, scaleY, scaleZ);

        }
        itemBeingScaled.transform.localScale = newScale;
        //Set scale value in DragAndDropHandler
        itemBeingScaled.GetComponent<DragAndDropHandler>().scale = newScale;

        this.transform.localScale = HandlerScale/newDirection.magnitude*initialDirection.magnitude;
        referenceObject.transform.localScale = HandlerScale / newDirection.magnitude * initialDirection.magnitude;
        
    }

    private void OnMouseUp()
    {
        /* if (videoStreamBackground.activeSelf)
         {
             videoStreamBackground.transform.parent.GetComponent<MoveObject>().dragged = false;
             videoStreamBackground.transform.parent.GetComponent<MoveObject>().endDrag = true;
         }*/

        if (transform.parent.parent.tag == "light")
        {
            Debug.Log("Light position is: " + itemBeingScaled.transform.position);
            Debug.Log("Light rotation is: " + itemBeingScaled.transform.rotation.eulerAngles);
            //itemBeingScaled.transform.parent.parent.gameObject.GetComponent<DragAndDropHandler>().dragged = false;
        }
        else
        {
            Debug.Log("Set position is: " + itemBeingScaled.transform.position);
            Debug.Log("Set rotation is: " + itemBeingScaled.transform.rotation.eulerAngles);
 
            Debug.Log("handleDragged = FALSE");
            handleDragged = false;
        }


        itemBeingDragged = null;
        itemBeingScaled = null;

        var setsList = GameObject.FindGameObjectsWithTag("set");
        foreach (var set in setsList)
        {
            set.GetComponent<DragAndDropHandler>().dragged = false;
            set.GetComponent<DragAndDropHandler>().endDrag = true;
        }

    }

    private static Vector3 GetMouseLampPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, 100);
        return new Vector3(hit.point.x, hit.point.y, 1);
    }
}

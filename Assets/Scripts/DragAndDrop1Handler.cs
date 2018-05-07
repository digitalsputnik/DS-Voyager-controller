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
        itemBeingScaled = transform.parent.gameObject; //Light
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

        //Position
        var T = (newDirection  - initialDirection)/initialDirection.magnitude*initialLightRelativePosition.magnitude;
        var newLightPosition = initialLightAbsolutePosition + T;
        itemBeingScaled.transform.position = newLightPosition;

        //Scale
        var scale = initialScale;
        scale = initialScale / initialDirection.magnitude * newDirection.magnitude;
        itemBeingScaled.transform.localScale = scale;
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

        itemBeingDragged = null;
        itemBeingScaled = null;

    }

    private static Vector3 GetMouseLampPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, 100);
        return new Vector3(hit.point.x, hit.point.y, 1);
    }
}

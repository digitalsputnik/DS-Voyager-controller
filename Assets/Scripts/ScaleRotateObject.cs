using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ScaleRotateObject : MonoBehaviour
{

    public GameObject referenceObject;
    public GameObject TrashCan;
    public static GameObject itemBeingDragged;
    public static GameObject itemBeingScaled;
    public bool handleDragged = false;

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
        itemBeingDragged = gameObject; // Drag and drop handle's parent
        if (transform.parent.parent.gameObject.name == "VideoStreamBackground")
        {
            itemBeingScaled = transform.parent.parent.parent.gameObject; //Set0
            handleDragged = true;
            Debug.Log("handleDragged = TRUE");

            //var setId = Convert.ToInt32(itemBeingScaled.name.Substring(3, 1)) - 1;
            //GameObject.Find("Set"+setId).GetComponent<DragAndDropHandler>().dragged = true;

        }
        else
        {
            itemBeingScaled = transform.parent.parent.gameObject; //Lamp1 //Light
                                                                  //itemBeingScaled.transform.parent.parent.GetComponent<DragAndDropHandler>().dragged = true;
                                                                  //Debug.Log("Dragged: "+ itemBeingScaled.transform.parent.parent.GetComponent<DragAndDropHandler>().dragged);

        }

        var setsList = GameObject.FindGameObjectsWithTag("set");
        foreach (var set in setsList)
        {
            set.GetComponent<MoveObject>().dragged = true;
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
        //newLightRotation.eulerAngles.Set (newLightRotation.eulerAngles.x, initialLightRotation.eulerAngles.y, newLightRotation.eulerAngles.z);
        itemBeingScaled.transform.rotation = newLightRotation;

        //Position
        if (transform.parent.parent.tag == "light")
        {
            var T = (newDirection - initialDirection) / initialDirection.magnitude * initialLightRelativePosition.magnitude;
            var newLightPosition = initialLightAbsolutePosition + T;
            itemBeingScaled.transform.position = newLightPosition;
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

        this.transform.localScale = HandlerScale / newDirection.magnitude * initialDirection.magnitude;
        referenceObject.transform.localScale = HandlerScale / newDirection.magnitude * initialDirection.magnitude;

    }

    private void OnMouseUp()
    {
        if (transform.parent.parent.tag == "light")
        {
            Debug.Log("Light position is: " + itemBeingScaled.transform.position);
            Debug.Log("Light rotation is: " + itemBeingScaled.transform.rotation.eulerAngles);
        }
        else
        {
            Debug.Log("Set position is: " + itemBeingScaled.transform.position);
            Debug.Log("Set rotation is: " + itemBeingScaled.transform.rotation.eulerAngles);

            handleDragged = false;
            Debug.Log("handleDragged = FALSE");
        }

        itemBeingDragged = null;
        itemBeingScaled = null;

        var setsList = GameObject.FindGameObjectsWithTag("set");
        foreach (var set in setsList)
        {
            set.GetComponent<MoveObject>().dragged = false;
            set.GetComponent<MoveObject>().endDrag = true;
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

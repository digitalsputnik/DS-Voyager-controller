using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Net;

public class DragAndDropHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

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

    //For VideoStream
    public GameObject drawTools;
    public GameObject videoStreamBackground;
    public List<int> VideoPixels;
    WebCamTexture webcamTexture = null;
    public float PointX = 0.0f;
    public float PointY = 0.0f;

    //Transform
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {

            Ray ray2 = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit2;
            if (Physics.Raycast(ray2, out hit2, 100))
            {
                if (hit2.transform.name == "Set")
                {
                    detectedLampProperties = GameObject.Find("DetectedLampProperties").GetComponent<DetectedLampProperties>();
                    var allSetsList = detectedLampProperties.SetsList;

                    if (allSetsList.Count > 1)
                    {
                        var hitObject = hit2.transform.parent.gameObject; //Set0
                        Debug.Log("hitObject is: " + hitObject.name);
                        Set hitSet = allSetsList[Convert.ToInt32(hitObject.name.Substring(3, 1))];
                        Debug.Log("hitSet is: Set" + hitSet.setID);
                        Set selectedSet = null;

                        foreach (var set in allSetsList)
                        {
                            if (set.isSelected)
                            {
                                selectedSet = set;
                            }
                        }
                        GameObject selectedSetObject = GameObject.Find("Set" + selectedSet.setID);
                        Debug.Log("selectedSetObject is: " + selectedSetObject.name);
                        Debug.Log("selectedSet is: Set" + selectedSet.setID);
                        var hitPositionZ = hit2.transform.gameObject.transform.parent.position.z;


                        Vector3 frontPosition = new Vector3(hitObject.transform.position.x, hitObject.transform.position.y, selectedSetObject.transform.position.z);
                        Vector3 backPosition = new Vector3(selectedSetObject.transform.position.x, selectedSetObject.transform.position.y, hitPositionZ);

                        selectedSetObject.transform.Find("Set").Find("Highlight").gameObject.SetActive(false);
                        selectedSet.isSelected = false;
                        Debug.Log("SelectedSet Highlight turned off!");

                        selectedSetObject.transform.position = backPosition;
                        selectedSet.position = backPosition;
                        Debug.Log("Old set pushed back!");

                        hitObject.transform.position = frontPosition;
                        hitSet.position = frontPosition;
                        Debug.Log("Clicked set pulled forward!");

                        hitSet.isSelected = true;
                        hitObject.transform.Find("Set").Find("Highlight").gameObject.SetActive(true);
                        Debug.Log("Clicked set highlighted!");

                    }
                }

            }

        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //if (!setupMode.activeSelf)
        //  return;

        if (dragged)
            return;

        if (gameObject.transform.tag == "set")
        {
            if (gameObject.transform.Find("Set").Find("Handle1").Find("DragAndDrop1").GetComponent<DragAndDrop1Handler>().handleDragged == true)
            {
                return;
            }
        }

        endDrag = false;
        TrashCan.SetActive(true);
        itemBeingDragged = gameObject;
        difference = transform.position - GetMouseLampPosition();
        if (drawTools.GetComponent<DrawScripts>().webcamTexture != null)
        {
            //Debug.Log("Found WebCamTexture!!!");
            webcamTexture = drawTools.GetComponent<DrawScripts>().webcamTexture;
        }
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
            if (gameObject.transform.Find("Set").Find("Handle1").Find("DragAndDrop1").GetComponent<DragAndDrop1Handler>().handleDragged == true)
            {
                return;
            }
        }

        transform.position = GetMouseLampPosition() + difference;
        position = transform.position;

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

        if (dragged || endDrag)
        {
            return;
        }

        if (gameObject.transform.tag == "set")
        {
            if (gameObject.transform.Find("Set").Find("Handle1").Find("DragAndDrop1").GetComponent<DragAndDrop1Handler>().handleDragged == true)
            {
                return;
            }
        }

        itemBeingDragged = null;
        transform.position = GetMouseLampPosition() + difference;


        //If droped on the bin, delete
        if (binRect.Contains(Input.mousePosition))
        {
            detectedLampProperties = GameObject.Find("DetectedLampProperties").GetComponent<DetectedLampProperties>();

            if (gameObject.tag == "set")
            {
                var setsList = detectedLampProperties.SetsList;
                Set setToRemove = null;
                float setPositionZ = 0.0f;
                foreach (var set in setsList)
                {
                    Debug.Log("Inside FOR loop....");
                    if (set.setID == Convert.ToInt32(gameObject.name.Substring(3, 1)))
                    {
                        setToRemove = set;
                        setPositionZ = set.position.z;
                        Debug.Log("Set To Remove Found!!!!");
                        Debug.Log("SetPositionZ is: " + setPositionZ.ToString());
                    }
                }
                foreach (var set2 in setsList)
                {
                    Debug.Log("Inside FOR loop...again!");
                    Debug.Log("set.position.z is: " + set2.position.z.ToString());
                    Debug.Log("set.position.z - 0.5 is: " + (set2.position.z - 0.5f).ToString());
                    if ((set2.position.z - 0.5f) == setPositionZ)
                    {
                        set2.isSelected = true;
                        var nextSelected = GameObject.Find("Set" + set2.setID);
                        nextSelected.transform.Find("Set").Find("Highlight").gameObject.SetActive(true);
                        Debug.Log("Next set selected!!!!");

                    }
                }

                Debug.Log("Removing Set!!!!");
                setsList.Remove(setToRemove);

            }
            else
            {
                var setsList = detectedLampProperties.SetsList;
                LampProperties lampToRemove = null;
                foreach (var set in setsList)
                {
                    if (set.isSelected)
                    {
                        var lampsList = set.lampslist;
                        foreach (var lamp in lampsList)
                        {
                            if (lamp.IP == this.GetComponent<Ribbon>().IP)
                            {
                                lampToRemove = lamp;
                            }
                        }
                        lampsList.Remove(lampToRemove);
                        Debug.Log("Lamp Removed................!!!");
                    }
                }
            }
            binImage.color = Color.white;
            IPAddress thisIP = IPAddress.Parse(this.GetComponent<Ribbon>().IP);
            if (setupScripts.LampIPtoLengthDictionary.ContainsKey(thisIP))
            {
                setupScripts.LampIPtoLengthDictionary.Remove(thisIP);
            }

            //Remove controlled pixels from strokes and animation
            animSender.RemoveLampFromStrokes(this.transform, thisIP.ToString());
            Destroy(this.gameObject);
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

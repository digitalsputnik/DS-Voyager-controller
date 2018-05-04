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

    //For VideoStream
    public GameObject drawTools;
    public GameObject videoStreamBackground;


    public Dictionary<int, Color> VideoPixels;
    WebCamTexture webcamTexture = null;


    void Start() {
        Debug.Log("Starting.....................");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //if (!setupMode.activeSelf)
          //  return;

        TrashCan.SetActive(true);
        itemBeingDragged = gameObject;
        difference = transform.position - GetMouseLampPosition();
        if (drawTools.GetComponent<DrawScripts>().webcamTexture != null)
        {
            //Debug.Log("Found WebCamTexture!!!");
            webcamTexture = drawTools.GetComponent<DrawScripts>().webcamTexture;
            VideoPixels = new Dictionary<int, Color>();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        //if (!setupMode.activeSelf)
          //  return;

        //Pick webCamTexture pixels
        if (webcamTexture != null)
        {
            //Find total number of lamp pixels
            var totalLampPixels = this.gameObject.GetComponent<Ribbon>().pipeLength;
            //Get VideoPlayer Rect
            var videoRect = videoStreamBackground.GetComponent<Renderer>().bounds;

            //Make sure dictionary is reinitialized
            VideoPixels.Clear();

            for (int i = 0; i < totalLampPixels; i++)
            {
                string pixelName = "pixel" + i;
                var lampPixelLED = transform.Find(pixelName).Find("LEDmodule");
                var lampPixelCenter = lampPixelLED.GetComponent<Renderer>().bounds.center;
                //Vector3 videoPixel = new Vector3(lampPixel.Find("LEDmodule").position.x, lampPixel.Find("LEDmodule").position.y, 0.7f );
                //Debug.Log("Bounding box min max is: " + videoRect.min+", "+videoRect.max);
                //Debug.Log("lampPixel position is: " + lampPixelCenter);
                if (lampPixelCenter.x >= videoRect.min.x && lampPixelCenter.x <= videoRect.max.x)
                {
                    if (lampPixelCenter.y >= videoRect.min.y && lampPixelCenter.y <= videoRect.max.y)
                    {
                        //Debug.Log("Value is within bounds!");
                        //var videoPixelColor = webcamTexture.GetPixel(Convert.ToInt32(lampPixelLED.position.x), Convert.ToInt32(lampPixelLED.position.y));
                        VideoPixels.Add(i, webcamTexture.GetPixel(Convert.ToInt32(lampPixelLED.position.x), Convert.ToInt32(lampPixelLED.position.y)));
                        //Debug.Log("Pixel: " + lampPixelLED.transform.parent.name + "  Color: " + videoPixelColor.ToString());
                    }
                    else
                    {
                        Debug.Log("ValueY out of bounds!!!");
                    }
                }
                else
                {
                    Debug.Log("ValueX out of bounds!!!");
                }

            }
        }
        else
        {
            Debug.Log("Video not running!!!");
        }

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

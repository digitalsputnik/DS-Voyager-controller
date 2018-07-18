using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Voyager.Lamps;

public class Trashcan : MonoBehaviour {

	[SerializeField] Color NormalColor, DeleteColor;
	LampManager lampManager;
	PhysicalLamp movingLamp;
	Image image;
	bool readyToDelete;
	Rect rect;

	void Start()
	{
		lampManager = GameObject.FindWithTag("LampManager").GetComponent<LampManager>();
		image = GetComponent<Image>();
		image.color = NormalColor;
		RectTransform menuCanvas = GameObject.Find("MenuCanvas").GetComponent<RectTransform>();
		rect = new Rect(new Vector2( image.rectTransform.position.x,
		                             image.rectTransform.position.y -
		                            (image.rectTransform.sizeDelta.y * menuCanvas.localScale.y) / 2),
		                new Vector2((image.rectTransform.sizeDelta.x * menuCanvas.localScale.x),
		                            (image.rectTransform.sizeDelta.y * menuCanvas.localScale.y)));
        
	}

	void Update()
	{
		CheckMovingLamps();
        
		if(movingLamp != null)
		{
			image.enabled = true;
			CheckLampRemove();
		}
		else
			image.enabled = false;
	}

	void CheckMovingLamps()
	{
		foreach(Lamp lamp in lampManager.GetLampsInWorkplace())
		{
			if (lamp.physicalLamp.MovingInWorkspace)
			{
				movingLamp = lamp.physicalLamp;
				return;
			}
        }

		movingLamp = null;
	}

    void CheckLampRemove()
	{
		if(Input.touchCount == 1)
		{
			Touch touch = Input.GetTouch(0);
			if(rect.Contains(touch.position))
			{
				image.color = DeleteColor;
				if (touch.phase == TouchPhase.Ended)
					lampManager.DestroyLamp(movingLamp);
            }
            else
            {
				image.color = NormalColor;
            }
        }
        else if(rect.Contains(Input.mousePosition))
        {
			image.color = DeleteColor;
			if (Input.GetMouseButtonUp(0))
				lampManager.DestroyLamp(movingLamp);
		}
		else
		{
			image.color = NormalColor;
		}
	}
}
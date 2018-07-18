using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraMove : MonoBehaviour {

	// TODO : Zooming in and out with touch
    //        Moving and zooming with macbook touchpad

	[SerializeField] float mouseScrollSpeed;
	[SerializeField] float minZoom, maxZoom;
	[SerializeField] string backgroundTag;

    bool movingWithMouse;
	bool movingWithTouch;
	bool zoomingWithTouch;

	Vector3 moveStartPosition;
	float startTouchDistance;
	float distanceToFOV;
   
	Camera cam;
	float camZPos;
	[SerializeField] GameObject setupModeObject;

	void Start()
	{
		cam = Camera.main;
		camZPos = cam.transform.position.z;
	}

	void Update()
	{
		if (Application.platform == RuntimePlatform.Android ||
            Application.platform == RuntimePlatform.IPhonePlayer)
        {
            CheckMoveOnTouch();
            CheckZoomOnTouch();
        }
        else
        {
			if(setupModeObject.activeSelf)
			{
				CheckMoveOnMouse();
                CheckScrollOnMouse();
			}
        }

        cam.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, camZPos);
	}

	void CheckZoomOnTouch()
	{
		if(Input.touchCount == 2)
		{
			Touch[] touches = new Touch[2];
			touches[0] = Input.GetTouch(0);
			touches[1] = Input.GetTouch(1);
            
			if (IsTouchOverUIObject(touches[0]) || IsTouchOverUIObject(touches[1]))
                return;

			if(!zoomingWithTouch)
			{
				startTouchDistance = Vector3.Distance(touches[1].position, touches[0].position);
				distanceToFOV = cam.fieldOfView / startTouchDistance;
				zoomingWithTouch = true;
				movingWithTouch = false;
			}

			Ray ray;
            RaycastHit hit;

			Vector3 screenCenter = touches[1].position + (touches[0].position - touches[1].position) / 2f;

			ray = cam.ScreenPointToRay(screenCenter);
			Physics.Raycast(ray, out hit, Mathf.Infinity, ~0);
			Vector3 worldPosBefore = hit.point;

			float distance = startTouchDistance - Vector3.Distance(touches[0].position, touches[1].position);
			cam.fieldOfView = (distance + startTouchDistance) * distanceToFOV;
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, minZoom, maxZoom);

			ray = cam.ScreenPointToRay(screenCenter);
            Physics.Raycast(ray, out hit, Mathf.Infinity, ~0);
            Vector3 worldPosAfter = hit.point;

			Vector3 worldMoveDelta = worldPosAfter - worldPosBefore;
			cam.transform.position = cam.transform.position - worldMoveDelta;

			if (touches[0].phase == TouchPhase.Ended || touches[1].phase == TouchPhase.Ended)
				zoomingWithTouch = false;
		}
	}

	void CheckMoveOnTouch()
	{
		if (Input.touchCount == 2)
        {         
			Touch[] touches = new Touch[2];
			touches[0] = Input.GetTouch(0);
			touches[1] = Input.GetTouch(1);
            
			if (IsTouchOverUIObject(touches[0]) || IsTouchOverUIObject(touches[1]))
            {
                movingWithTouch = true;
                return;
            }

			Ray ray;
            RaycastHit hit;
			Vector3 screenCenter = touches[1].position + (touches[0].position - touches[1].position) / 2f;
			
			ray = cam.ScreenPointToRay(screenCenter);
			Physics.Raycast(ray, out hit, Mathf.Infinity, ~0);
			Vector3 worldPosBefore = hit.point;

            if (!movingWithTouch)
            {            
				if (hit.transform.tag == backgroundTag)
				{
					moveStartPosition = hit.point;
					movingWithTouch = true;
				}
			}
			else
			{
				if (touches[0].phase == TouchPhase.Ended || touches[0].phase == TouchPhase.Canceled ||
				    touches[1].phase == TouchPhase.Ended || touches[1].phase == TouchPhase.Canceled  )
					movingWithTouch = false;
				else
				{
					Vector3 hitOffset = moveStartPosition - hit.point;
					cam.transform.position = cam.transform.position + hitOffset;
				}
			}
		}
		else
			movingWithTouch = false;
	}

	void CheckScrollOnMouse()
	{
		if (EventSystem.current.IsPointerOverGameObject())
            return;

		float scroll = Input.GetAxis("Mouse ScrollWheel");
		if (scroll.Equals(0.0f))
			return;
		
		RaycastHit hit;

        Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, ~0);
        Vector3 worldMousePos = hit.point;
              
		cam.fieldOfView -= scroll * mouseScrollSpeed;
		cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, minZoom, maxZoom);

        Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, ~0);
        Vector3 newWorldMousePos = hit.point;
      
        Vector3 newScrollDelta = newWorldMousePos - worldMousePos;
		cam.transform.position = cam.transform.position - newScrollDelta;
	}

	void CheckMoveOnMouse()
	{      
		RaycastHit hit;
        Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity, ~0);

		if(Input.GetMouseButtonDown(0) && !movingWithMouse)
		{
            if (EventSystem.current.IsPointerOverGameObject())
                return;
			
			if (hit.transform.tag == backgroundTag)
            {
                moveStartPosition = hit.point;
                movingWithMouse = true;
            }
		}
		if(movingWithMouse)
		{
            if (Input.GetMouseButton(0))
			{            
				Vector3 hitOffset = moveStartPosition - hit.point;
				cam.transform.position = cam.transform.position + hitOffset;
			}
			else if(Input.GetMouseButtonUp(0))
				movingWithMouse = false;
        }
	}

	bool IsTouchOverUIObject(Touch touch)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
		eventDataCurrentPosition.position = touch.position;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}
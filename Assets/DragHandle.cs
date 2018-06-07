using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DragHandle : MonoBehaviour {

	public bool Dragging;
	Vector3 dragOffset;
	int activeTouch;
       
	public event EventHandler OnDragStarted;
	public event EventHandler OnDragging;
	public event EventHandler OnDragEnded;

	void Update()
	{
		if (Input.touchCount > 0)
		{
			if (!Dragging)
				CheckIncomingTouches();
			else
				HandleRegistedTouch();
		}
		else
		{
			if (IsMouseInUse())
                return;
            Dragging = false;
		}
	}

    void CheckIncomingTouches()
	{
		for (int t = 0; t < Input.touches.Length; t++)
        {
            Touch touch = Input.GetTouch(t);

            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform.gameObject == this.gameObject)
                    {
						Plane plane = new Plane(Vector3.back, Vector3.zero);
						float distance;
						plane.Raycast(ray, out distance);                  
                        Dragging = true;
                        activeTouch = t;
						dragOffset = transform.position - ray.GetPoint(distance);
                        if (OnDragStarted != null) OnDragStarted(this, new EventArgs());
                        return;
                    }
                }
            }
        }
	}

	void HandleRegistedTouch()
	{
		if (Input.touches.Length >= activeTouch)
		{
			Touch touch = Input.GetTouch(activeTouch);
			
			if (touch.phase == TouchPhase.Moved)
			{
				Ray ray = Camera.main.ScreenPointToRay(touch.position);
				Plane hPlane = new Plane(Vector3.back, Vector3.zero);
				float distance = 0;
				
				if (hPlane.Raycast(ray, out distance))
				{
					transform.position = ray.GetPoint(distance) + dragOffset;
					if (OnDragging != null) OnDragging(this, new EventArgs());
				}
			}
			else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
			{
				Dragging = false;
				if (OnDragEnded != null) OnDragEnded(this, new EventArgs());
			}
		}
	}

    bool IsMouseInUse()
    {
        if(Input.GetMouseButtonDown(0) && !Dragging)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.gameObject == this.gameObject)
                {
                    Plane plane = new Plane(Vector3.back, Vector3.zero);
                    float distance;
                    plane.Raycast(ray, out distance);
                    Dragging = true;
                    dragOffset = transform.position - ray.GetPoint(distance);
                    if (OnDragStarted != null) OnDragStarted(this, new EventArgs());
                    return true;
                }
            }
        }
        else if (Input.GetMouseButton(0) && Dragging)
        {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane hPlane = new Plane(Vector3.back, Vector3.zero);
            float distance = 0;

            if (hPlane.Raycast(ray, out distance))
            {
                transform.position = ray.GetPoint(distance) + dragOffset;
                if (OnDragging != null) OnDragging(this, new EventArgs());
				return true;
            }
        }
        else if (Input.GetMouseButtonUp(0) && Dragging)
        {
            Dragging = false;
            if (OnDragEnded != null) OnDragEnded(this, new EventArgs());
            return true;
        }

        return false;
    }
}

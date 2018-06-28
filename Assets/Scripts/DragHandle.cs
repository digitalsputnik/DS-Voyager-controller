using UnityEngine;

public class DragHandle : MonoBehaviour {

	public bool BeingDragged;
	public DragPhase Phase { get; private set; }

	Vector3 dragOffset;
	int activeTouch;

	public delegate void DragEvent();
	public event DragEvent OnDragStarted;
	public event DragEvent OnDragging;
	public event DragEvent OnDragEnded;

	void Update()
	{
		if (Input.touchCount > 0)
		{
			if (!BeingDragged)
				CheckIncomingTouches();
			else
				HandleRegistedTouch();
		}
		else
		{
			if (IsMouseInUse())
                return;
            BeingDragged = false;
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
                        BeingDragged = true;
                        activeTouch = t;
						dragOffset = transform.position - ray.GetPoint(distance);
                        if (OnDragStarted != null) OnDragStarted();
						Phase = DragPhase.Begin;
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
					if (OnDragging != null) OnDragging();
					Phase = DragPhase.Moved;
				}
			}
			else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
			{
				BeingDragged = false;
				if (OnDragEnded != null) OnDragEnded();
				Phase = DragPhase.Ended;
			}
		}
	}

    bool IsMouseInUse()
    {
        if(Input.GetMouseButtonDown(0) && !BeingDragged)
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
                    BeingDragged = true;
                    dragOffset = transform.position - ray.GetPoint(distance);
                    if (OnDragStarted != null) OnDragStarted();
					Phase = DragPhase.Begin;
                    return true;
                }
            }
        }
        else if (Input.GetMouseButton(0) && BeingDragged)
        {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane hPlane = new Plane(Vector3.back, Vector3.zero);
            float distance = 0;

            if (hPlane.Raycast(ray, out distance))
            {
                transform.position = ray.GetPoint(distance) + dragOffset;
                if (OnDragging != null) OnDragging();
				Phase = DragPhase.Moved;
				return true;
            }
        }
        else if (Input.GetMouseButtonUp(0) && BeingDragged)
        {
            BeingDragged = false;
            if (OnDragEnded != null) OnDragEnded();
			Phase = DragPhase.Ended;
            return true;
        }

        return false;
    }
}

public enum DragPhase
{
	Begin, Moved, Ended
}

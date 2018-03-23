using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PanZoom : MonoBehaviour {

	public float maxZoom = 90.0f;
	public float minZoom = 30.0f;
	public float mouseZoomSpeed = 1.0f;
	public float pinchZoomSpeed = 0.1f; 
	public float panSpeed = -40.0f;

	Vector3 bottomLeft;
	Vector3 topRight;
	private Vector3 lastPosition;
	private Vector3 touchOrigin;

	float cameraMaxY;
	float cameraMinY;
	float cameraMaxX;
	float cameraMinX;

	public Dropdown toolsDropDown;


	void Start()
	{
		//set max camera bounds
		topRight = Camera.main.ScreenToWorldPoint(new Vector3(Camera.main.pixelWidth, Camera.main.pixelHeight, -transform.position.z));
		bottomLeft = Camera.main.ScreenToWorldPoint(new Vector3(0,0,-transform.position.z));
		cameraMaxX = topRight.x + 30;
		cameraMaxY = topRight.y + 30;
		cameraMinX = bottomLeft.x - 30;
		cameraMinY = bottomLeft.y - 30;
	}

	public bool IsMouseOverUIObject()
	{
		    bool result = EventSystem.current.currentSelectedGameObject != null;
		    //Debug.Log("Is the mouse over a UI object?  Answer: " + result);
		 
		    return result;
	}

	void Update ()
	{
		//TODO: Check if on setup mode!
		//if (toolsDropDown.value == 2) {

		// Click and drag to pan
/*		if (Input.touchCount == 2 && Input.GetMouseButtonDown (0)) {
			// Get mouse origin
			lastPosition = Input.mousePosition;
		}
		*/		

/*		if (Input.touchCount < 2 && Input.GetMouseButton (0)) 
		{

			// check for ui element touch
			if(IsMouseOverUIObject())
			{
				return;
			}

			// Cast a ray to make sure user has not clicked/touched a draggable object
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit, 100) && hit.transform.tag != "lamp" && hit.transform.tag != "handle") {
				// Translate camera position
				Vector3 pos = Camera.main.ScreenToViewportPoint (Input.mousePosition - lastPosition);
				Vector3 move = new Vector3 (pos.x * panSpeed, pos.y * panSpeed, 0);
				transform.Translate (move, Space.Self);
				lastPosition = Input.mousePosition;
			} 

			// Check if camera is out-of-bounds, if so, move back in-bounds
			topRight = Camera.main.ScreenToWorldPoint (new Vector3 (Camera.main.pixelWidth, Camera.main.pixelHeight, -transform.position.z));
			bottomLeft = Camera.main.ScreenToWorldPoint (new Vector3 (0, 0, -transform.position.z));

			if (topRight.x > cameraMaxX) {
				transform.position = new Vector3 (transform.position.x - (topRight.x - cameraMaxX), transform.position.y, transform.position.z);
			}

			if (topRight.y > cameraMaxY) {
				transform.position = new Vector3 (transform.position.x, transform.position.y - (topRight.y - cameraMaxY), transform.position.z);
			}

			if (bottomLeft.x < cameraMinX) {
				transform.position = new Vector3 (transform.position.x + (cameraMinX - bottomLeft.x), transform.position.y, transform.position.z);
			}

			if (bottomLeft.y < cameraMinY) {
				transform.position = new Vector3 (transform.position.x, transform.position.y + (cameraMinY - bottomLeft.y), transform.position.z);
			}

		} */

		// Zoom in/out with mouse wheel
		if ((Input.GetAxis ("Mouse ScrollWheel") > 0) && Camera.main.fieldOfView > minZoom) {
			Camera.main.fieldOfView = Camera.main.fieldOfView - mouseZoomSpeed;
		}

		if ((Input.GetAxis ("Mouse ScrollWheel") < 0) && Camera.main.fieldOfView < maxZoom) {
			Camera.main.fieldOfView = Camera.main.fieldOfView + mouseZoomSpeed;
		}

//#if UNITY_ANDROID
//			}
//#endif


		if (Input.touchCount == 2) { // If there are two touches on the device

			// Store both touches.
			Touch tZero = Input.GetTouch (0);
			Touch tOne = Input.GetTouch (1);

			Vector3 touchZero = new Vector3 (tZero.position.x, tZero.position.y);
			Vector3 touchOne = new Vector3 (tOne.position.x, tOne.position.y);

			if (tOne.phase == TouchPhase.Began) {
				// Get mouse origin
				lastPosition = touchOne;
			}

			//PAN
			// check for ui element touch
			if(IsMouseOverUIObject())
			{
				return;
			}

			// Cast a ray to make sure user has not clicked/touched a draggable object
			Ray ray = Camera.main.ScreenPointToRay (touchOne);
			RaycastHit hit;
			if (Physics.Raycast (ray, out hit, 100) && hit.transform.tag != "lamp" && hit.transform.tag != "handle") {
				// Translate camera position
				Vector3 pos = Camera.main.ScreenToViewportPoint (touchOne - lastPosition);

				Vector3 move = new Vector3 (pos.x * panSpeed, pos.y * panSpeed, 0);
				transform.Translate (move, Space.Self);
				lastPosition = touchOne;
			} 

			// Check if camera is out-of-bounds, if so, move back in-bounds
			topRight = Camera.main.ScreenToWorldPoint (new Vector3 (Camera.main.pixelWidth, Camera.main.pixelHeight, -transform.position.z));
			bottomLeft = Camera.main.ScreenToWorldPoint (new Vector3 (0, 0, -transform.position.z));

			if (topRight.x > cameraMaxX) {
				transform.position = new Vector3 (transform.position.x - (topRight.x - cameraMaxX), transform.position.y, transform.position.z);
			}

			if (topRight.y > cameraMaxY) {
				transform.position = new Vector3 (transform.position.x, transform.position.y - (topRight.y - cameraMaxY), transform.position.z);
			}

			if (bottomLeft.x < cameraMinX) {
				transform.position = new Vector3 (transform.position.x + (cameraMinX - bottomLeft.x), transform.position.y, transform.position.z);
			}

			if (bottomLeft.y < cameraMinY) {
				transform.position = new Vector3 (transform.position.x, transform.position.y + (cameraMinY - bottomLeft.y), transform.position.z);
			}



			//ZOOM
			// Find the position in the previous frame of each touch.
			Vector2 touchZeroPrevPos = tZero.position - tZero.deltaPosition;
			Vector2 touchOnePrevPos = tOne.position - tOne.deltaPosition;

			// Find the magnitude of the vector (the distance) between the touches in each frame.
			float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
			float touchDeltaMag = (touchZero - touchOne).magnitude;

			// Find the difference in the distances between each frame.
			float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

			// Otherwise change the field of view based on the change in distance between the touches.
			Camera.main.fieldOfView += deltaMagnitudeDiff * pinchZoomSpeed;

			// Clamp the field of view to make sure it's between 0 and 180.
			Camera.main.fieldOfView = Mathf.Clamp (Camera.main.fieldOfView, 0.1f, 179.9f);

		}
	}

	//}


}

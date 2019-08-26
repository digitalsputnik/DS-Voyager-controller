using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace VoyagerApp.UI
{
    public class CameraMove : MonoBehaviour
    {
        [SerializeField] float minSize = 5.0f;
        [SerializeField] float maxSize = 30.0f;
        [SerializeField] float zoomSpeed = 5.0f;

        Camera cam;
        Vector2 mouseStartPosition;
        Vector2 touchStartPosition;
        bool moving;
        bool movementRestriction;

        Touch touchZero;
        Touch touchOne;
        Touch touchTwo;

        public float orthoZoomSpeed = 1f;

        void Start()
        {
            cam = GetComponent<Camera>();
            orthoZoomSpeed = 1f;
        }

        void Update()
        {
            if (Application.isMobilePlatform)
            {
                HandleTouch();
            }
            else
                HandleMouse();
        }

        void HandleTouch()
        {
            if (Input.touchCount == 1)
            {
                //Store first touch
                touchZero = Input.GetTouch(0);

                //Get startPos and check if over UI
                if (touchZero.phase == TouchPhase.Began)
                {
                    movementRestriction = true;

                    touchStartPosition = cam.ScreenToWorldPoint(touchZero.position);
                    moving = !IsPointerOverAnythingTouch(touchStartPosition);
                }
            }

            if (touchZero.phase == TouchPhase.Moved && Input.touchCount < 2 && movementRestriction != false) // Prevents moving when zooming
            {
                if (moving)
                {
                    Vector2 position = cam.ScreenToWorldPoint(touchZero.position);
                    Vector2 delta = touchStartPosition - position;
                    transform.Translate(delta);
                }
            }

            if (Input.touchCount >= 2)
            {
                if (moving) // Prevents scene scrolling when on UI
                {
                    touchOne = Input.GetTouch(1);
                    // Find the position in the previous frame of each touch.
                    Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                    Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                    // Find the magnitude of the vector (the distance) between the touches in each frame.
                    float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                    float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                    // Find the difference in the distances between each frame.
                    float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

                    //Handles camera zoom
                    float step = (deltaMagnitudeDiff / 100) * orthoZoomSpeed;
                    float size = cam.orthographicSize + step;
                    cam.orthographicSize = Mathf.Clamp(size, minSize, maxSize);
                    movementRestriction = false;
                }
            }
        }

        void HandleMouse()
        {
            // Prevents UI and Game scene from scrolling at the same time
            if (Input.GetMouseButtonDown(0) || Mathf.Abs(Input.mouseScrollDelta.y) > 0.0001f)
            {
                mouseStartPosition = cam.ScreenToWorldPoint(Input.mousePosition);
                moving = !IsPointOverAnything(mouseStartPosition);
            }

            if (Input.GetMouseButton(0))
            {
                if (moving)
                {
                    Vector2 position = cam.ScreenToWorldPoint(Input.mousePosition);
                    Vector2 delta = mouseStartPosition - position;
                    transform.Translate(delta);
                }
            }

            if (Mathf.Abs(Input.mouseScrollDelta.y) > 0.0001f)
            {
                if (moving) // Prevents scene scrolling when on UI
                {
                    float step = Input.mouseScrollDelta.y * zoomSpeed * -1;
                    float size = cam.orthographicSize + step;
                    cam.orthographicSize = Mathf.Clamp(size, minSize, maxSize);
                }
            }
        }
        // Use for mouse
        bool IsPointOverAnything(Vector2 screenPoint)
        {
            RaycastHit2D hit = Physics2D.Raycast(screenPoint, Vector2.zero);
            bool overObject = hit.collider != null;
            bool overUI = EventSystem.current.IsPointerOverGameObject();

            return overObject || overUI;
        }

        // Use for touch
        bool IsPointerOverAnythingTouch(Vector2 screenPoint)
        {
            RaycastHit2D hit = Physics2D.Raycast(screenPoint, Vector2.zero);
            bool overObject = hit.collider != null;
            bool overUI = EventSystem.current.IsPointerOverGameObject(touchZero.fingerId);

            return overObject || overUI;
        }
    }
}
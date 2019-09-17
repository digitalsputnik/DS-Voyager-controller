using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace VoyagerApp.UI
{
    public class CameraMove : MonoBehaviour
    {
        public static CameraMoveState state = CameraMoveState.None;
        public static Vector2 pointerPosition;

        [SerializeField] float minSize = 5.0f;
        [SerializeField] float maxSize = 30.0f;
        [SerializeField] float mouseZoomSpeed = 5.0f;
        [SerializeField] float touchZoomSpeed = 0.05f;

        Camera cam;

        Vector2 pointerStart;

        float prevTouchDistance;
        float touchDistanceDelta;

        void Start() => cam = GetComponent<Camera>();

        void Update()
        {
            if (Application.isMobilePlatform)
                HandleTouch();
            else
                HandleMouse();

            HandleState();
        }

        void HandleMouse()
        {
            if (!Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    var screenPos = cam.ScreenToWorldPoint(Input.mousePosition);
                    if (IsPointOverAnything(screenPos))
                        state = CameraMoveState.WorkspaceOverItem;
                    else
                        state = CameraMoveState.WorkspaceClear;
                }
                else if (Input.GetMouseButtonUp(0))
                    state = CameraMoveState.None;
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    pointerStart = cam.ScreenToWorldPoint(Input.mousePosition);
                    state = CameraMoveState.CameraMove;
                }
                else if (Input.GetMouseButtonUp(0))
                    state = CameraMoveState.None;
            }

            if (state == CameraMoveState.None)
            {
                if (Mathf.Abs(Input.mouseScrollDelta.y) > 0.0001f)
                    state = CameraMoveState.CameraMouseZoom;
            }
            else if (state == CameraMoveState.CameraMouseZoom)
            {
                if (Mathf.Abs(Input.mouseScrollDelta.y) < 0.0001f)
                    state = CameraMoveState.None;
            }

            if (state != CameraMoveState.None)
                pointerPosition = cam.ScreenToWorldPoint(Input.mousePosition);
        }

        void HandleTouch()
        {
            if (Input.touchCount == 1)
            {
                if (state == CameraMoveState.CameraMove ||
                    state == CameraMoveState.CameraPanAndZoom)
                    state = CameraMoveState.None;
                else
                {
                    var touch = Input.GetTouch(0);
                    if (touch.phase == TouchPhase.Began)
                    {
                        var screenPos = cam.ScreenToWorldPoint(touch.position);
                        if (IsPointerOverAnythingTouch(screenPos))
                            state = CameraMoveState.WorkspaceOverItem;
                        else
                            state = CameraMoveState.WorkspaceClear;
                    }
                    else if (touch.phase == TouchPhase.Ended)
                        state = CameraMoveState.None;
                }
            }
            else if (Input.touchCount == 2)
            {
                if (state != CameraMoveState.CameraMove)
                {
                    var touch = Input.GetTouch(1);
                    pointerStart = cam.ScreenToWorldPoint(touch.position);
                    state = CameraMoveState.CameraMove;
                }
            }
            else if (Input.touchCount == 3)
            {
                if (state != CameraMoveState.CameraPanAndZoom)
                {
                    pointerStart = cam.ScreenToWorldPoint(GetTouchMiddle());
                    prevTouchDistance = GetTouchDistance();
                    state = CameraMoveState.CameraPanAndZoom;
                }
            }
            else
                state = CameraMoveState.None;

			if (state == CameraMoveState.WorkspaceClear)
				pointerPosition = cam.ScreenToWorldPoint(Input.GetTouch(0).position);

			if (state == CameraMoveState.CameraMove)
                pointerPosition = cam.ScreenToWorldPoint(Input.GetTouch(1).position);

            if (state == CameraMoveState.CameraPanAndZoom)
            {
                float distance = GetTouchDistance();
                touchDistanceDelta = distance - prevTouchDistance;
                prevTouchDistance = distance;
                pointerPosition = cam.ScreenToWorldPoint(GetTouchMiddle());
            }
        }

        Vector2 GetTouchMiddle()
        {
            var touch1 = Input.GetTouch(1).position;
            var touch2 = Input.GetTouch(2).position;
            return touch1 + (touch2 - touch1) / 2.0f;
        }

        float GetTouchDistance()
        {
            var touch1 = Input.GetTouch(1).position;
            var touch2 = Input.GetTouch(2).position;
            return Vector2.Distance(touch1, touch2);
        }

        void HandleState()
        {

            switch (state)
            {
                case CameraMoveState.CameraMove:
                    MoveCamera();
                    break;
                case CameraMoveState.CameraMouseZoom:
                    ZoomCamera();
                    break;
                case CameraMoveState.CameraPanAndZoom:
                    MoveCamera();
                    ZoomCamera();
                    break;
                default:
                    break;
            }
        }

        void MoveCamera()
        {
            Vector2 delta = pointerStart - pointerPosition;
            transform.Translate(delta);
        }

        void ZoomCamera()
        {
            float step = GetStep();
            float size = cam.orthographicSize + step;

            Vector3 before = WorldPoint();
            cam.orthographicSize = Mathf.Clamp(size, minSize, maxSize);
            Vector3 after = WorldPoint();

            transform.position = transform.position - (after - before);
        }

        float GetStep()
        {
            if (!Application.isMobilePlatform)
                return -Input.mouseScrollDelta.y * mouseZoomSpeed;

            if (Input.touchCount == 3)
                return -touchDistanceDelta * touchZoomSpeed * cam.orthographicSize;

            return 0.0f;
        }

        Vector3 WorldPoint()
        {
            if (!Application.isMobilePlatform)
                return cam.ScreenToWorldPoint(Input.mousePosition);

            if (Input.touchCount == 2)
                return cam.ScreenToWorldPoint(Input.GetTouch(1).position);

            if (Input.touchCount == 3)
                return cam.ScreenToWorldPoint(GetTouchMiddle());

            return Vector3.zero;
        }

        bool IsPointOverAnything(Vector2 screenPoint)
        {
            RaycastHit2D hit = Physics2D.Raycast(screenPoint, Vector2.zero);
            bool overObject = hit.collider != null;
            bool overUI = EventSystem.current.IsPointerOverGameObject();

            return overObject || overUI;
        }

        bool IsPointerOverAnythingTouch(Vector2 screenPoint)
        {
            RaycastHit2D hit = Physics2D.Raycast(screenPoint, Vector2.zero);
            bool overObject = hit.collider != null;
            bool overUI = EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);

            return overObject || overUI;
        }
    }

    // Maybe replace with bit enum, so you can get rid of pan and zoom.
    public enum CameraMoveState
    {
        WorkspaceOverItem,
        WorkspaceClear,
        CameraMove,
        CameraMouseZoom,
        CameraPanAndZoom,
        None
    }
}
﻿using UnityEngine;
using UnityEngine.EventSystems;

namespace VoyagerApp.UI
{
    public class CameraMove : MonoBehaviour
    {
        public static CameraMoveState state = CameraMoveState.None;
        public static Vector2 pointerPosition;
        public static bool Used;

        [SerializeField] float minSize = 5.0f;
        [SerializeField] float maxSize = 30.0f;
        [SerializeField] float mouseZoomSpeed = 5.0f;
        [SerializeField] float touchZoomSpeed = 0.05f;

        Camera cam;

        Vector2 pointerStart;

        float prevTouchDistance;
        float touchDistanceDelta;

        void Start()
        {
            cam = GetComponent<Camera>();
            Input.simulateMouseWithTouches = false;
        }

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
            if (ApplicationState.ControllingMode.value == ControllingMode.Items)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    var screenPos = cam.ScreenToWorldPoint(Input.mousePosition);
                    if (IsPointerOverUI())
                        state = CameraMoveState.None;
                    else if (IsPointOverObject(screenPos))
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
                    if (!IsPointerOverUI())
                        state = CameraMoveState.CameraMove;
                }
                else if (Input.GetMouseButtonUp(0))
                    state = CameraMoveState.None;
            }

            if (state == CameraMoveState.None)
            {
                if (Mathf.Abs(Input.mouseScrollDelta.y) > 0.0001f && !IsPointerOverUI())
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
            if (ApplicationState.ControllingMode.value == ControllingMode.Items)
            {
                if (Input.touchCount == 1)
                {
                    var touch = Input.GetTouch(0);
                    if (touch.phase == TouchPhase.Began)
                    {
                        var screenPos = cam.ScreenToWorldPoint(touch.position);
                        if (IsPointerOverUI())
                            state = CameraMoveState.None;
                        else if (IsPointOverObject(screenPos))
                            state = CameraMoveState.WorkspaceOverItem;
                        else
                            state = CameraMoveState.WorkspaceClear;
                    }
                    else if (touch.phase == TouchPhase.Ended)
                        state = CameraMoveState.None;
                }
                else
                    state = CameraMoveState.None;
            }
            else
            {
                if (ApplicationState.ControllingMode.value == ControllingMode.CameraToggled)
                {
                    if (Input.touchCount == 1)
                    {
                        if (state != CameraMoveState.CameraMove)
                        {
                            var touch = Input.GetTouch(0);
                            if (touch.phase == TouchPhase.Began)
                            {
                                pointerStart = cam.ScreenToWorldPoint(touch.position);
                                if (!IsPointerOverUI())
                                    state = CameraMoveState.CameraMove;
                            }
                            else if (touch.phase == TouchPhase.Ended)
                                state = CameraMoveState.None;
                        }
                    }
                    else if (Input.touchCount == 2)
                    {
                        if (state != CameraMoveState.CameraPanAndZoom)
                        {
                            if (!IsTouchOverUI(0) && !IsTouchOverUI(1))
                            {
                                pointerStart = cam.ScreenToWorldPoint(GetTouchMiddle(0, 1));
                                prevTouchDistance = GetTouchDistance(0, 1);
                                state = CameraMoveState.CameraPanAndZoom;
                            }
                        }
                    }
                    else
                        state = CameraMoveState.None;
                }
                else
                {
                    if (Input.touchCount == 2)
                    {
                        if (state != CameraMoveState.CameraMove)
                        {
                            var touch = Input.GetTouch(1);
                            if (touch.phase == TouchPhase.Began)
                            {
                                pointerStart = cam.ScreenToWorldPoint(touch.position);
                                if (!IsTouchOverUI(1))
                                    state = CameraMoveState.CameraMove;
                            }
                            else if (touch.phase == TouchPhase.Ended)
                                state = CameraMoveState.None;
                        }
                    }
                    else if (Input.touchCount == 3)
                    {
                        if (state != CameraMoveState.CameraPanAndZoom)
                        {
                            if (!IsTouchOverUI(1) && !IsTouchOverUI(2))
                            {
                                pointerStart = cam.ScreenToWorldPoint(GetTouchMiddle(1, 2));
                                prevTouchDistance = GetTouchDistance(1, 2);
                                state = CameraMoveState.CameraPanAndZoom;
                            }
                        }
                    }
                    else
                        state = CameraMoveState.None;
                }
            }

			if (state == CameraMoveState.WorkspaceClear || state == CameraMoveState.WorkspaceOverItem)
				pointerPosition = cam.ScreenToWorldPoint(Input.GetTouch(0).position);

			if (state == CameraMoveState.CameraMove)
            {
                if (ApplicationState.ControllingMode.value == ControllingMode.CameraToggled)
                    pointerPosition = cam.ScreenToWorldPoint(Input.GetTouch(0).position);
                else
                    pointerPosition = cam.ScreenToWorldPoint(Input.GetTouch(1).position);
            }

            if (state == CameraMoveState.CameraPanAndZoom)
            {
                if (ApplicationState.ControllingMode.value == ControllingMode.CameraToggled)
                {
                    float distance = GetTouchDistance(0, 1);
                    touchDistanceDelta = distance - prevTouchDistance;
                    prevTouchDistance = distance;
                    pointerPosition = cam.ScreenToWorldPoint(GetTouchMiddle(0, 1));
                }
                else
                {
                    float distance = GetTouchDistance(1, 2);
                    touchDistanceDelta = distance - prevTouchDistance;
                    prevTouchDistance = distance;
                    pointerPosition = cam.ScreenToWorldPoint(GetTouchMiddle(1, 2));
                }
            }
        }

        Vector2 GetTouchMiddle(int first, int second)
        {
            var touch1 = Input.GetTouch(first).position;
            var touch2 = Input.GetTouch(second).position;
            return touch1 + (touch2 - touch1) / 2.0f;
        }

        float GetTouchDistance(int first, int second)
        {
            var touch1 = Input.GetTouch(first).position;
            var touch2 = Input.GetTouch(second).position;
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

            if (ApplicationState.ControllingMode.value == ControllingMode.CameraToggled)
            {
                if (Input.touchCount == 2)
                    return -touchDistanceDelta * touchZoomSpeed * cam.orthographicSize;
            }
            else
            {
                if (Input.touchCount == 3)
                    return -touchDistanceDelta * touchZoomSpeed * cam.orthographicSize;
            }

            return 0.0f;
        }

        Vector3 WorldPoint()
        {
            if (!Application.isMobilePlatform)
                return cam.ScreenToWorldPoint(Input.mousePosition);

            if (ApplicationState.ControllingMode.value == ControllingMode.CameraToggled)
            {
                if (Input.touchCount == 1)
                    return cam.ScreenToWorldPoint(Input.GetTouch(0).position);

                if (Input.touchCount == 2)
                    return cam.ScreenToWorldPoint(GetTouchMiddle(0, 1));
            }
            else
            {
                if (Input.touchCount == 2)
                    return cam.ScreenToWorldPoint(Input.GetTouch(1).position);

                if (Input.touchCount == 3)
                    return cam.ScreenToWorldPoint(GetTouchMiddle(1, 2));
            }

            return Vector3.zero;
        }

        bool IsPointOverObject(Vector2 screenPoint)
        {
            RaycastHit2D hit = Physics2D.Raycast(screenPoint, Vector2.zero);
            return hit.collider != null;
        }

        bool IsPointerOverUI()
        {
            if (Application.isMobilePlatform)
            {
                for (int t = 0; t < Input.touchCount; t++)
                {
                    Touch touch = Input.GetTouch(t);
                    bool overUI = EventSystem.current.IsPointerOverGameObject(touch.fingerId);
                    if (overUI) return overUI; 
                }
            }
            else
                return EventSystem.current.IsPointerOverGameObject();

            return false;
        }

        bool IsTouchOverUI(int t)
        {
            Touch touch = Input.GetTouch(t);
            return EventSystem.current.IsPointerOverGameObject(touch.fingerId);
        }
    }

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
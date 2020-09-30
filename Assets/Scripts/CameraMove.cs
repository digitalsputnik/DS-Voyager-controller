using UnityEngine;
using UnityEngine.EventSystems;

namespace VoyagerController
{
    public class CameraMove : MonoBehaviour
    {
        public static CameraMoveState State = CameraMoveState.None;
        public static Vector2 PointerPosition;
        public static bool Used;

        [SerializeField] private float _minSize = 5.0f;
        [SerializeField] private float _maxSize = 30.0f;
        [SerializeField] private float _mouseZoomSpeed = 5.0f;
        [SerializeField] private float _touchZoomSpeed = 0.05f;

        private Camera cam;
        private Vector2 pointerStart;
        private float prevTouchDistance;
        private float touchDistanceDelta;

        private void Start()
        {
            cam = GetComponent<Camera>();
            Input.simulateMouseWithTouches = false;
        }

        private void Update()
        {
            if (Application.isMobilePlatform)
                HandleTouch();
            else
                HandleMouse();

            HandleState();
        }

        private void HandleMouse()
        {
            if (ApplicationState.ControlMode.Value == ControllingMode.Items)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    var screenPos = cam.ScreenToWorldPoint(Input.mousePosition);
                    if (IsPointerOverUI())
                        State = CameraMoveState.None;
                    else if (IsPointOverObject(screenPos))
                        State = CameraMoveState.WorkspaceOverItem;
                    else
                        State = CameraMoveState.WorkspaceClear;
                }
                else if (Input.GetMouseButtonUp(0))
                    State = CameraMoveState.None;
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    pointerStart = cam.ScreenToWorldPoint(Input.mousePosition);
                    if (!IsPointerOverUI())
                        State = CameraMoveState.CameraMove;
                }
                else if (Input.GetMouseButtonUp(0))
                    State = CameraMoveState.None;
            }

            if (State == CameraMoveState.None)
            {
                if (Mathf.Abs(Input.mouseScrollDelta.y) > 0.0001f && !IsPointerOverUI())
                    State = CameraMoveState.CameraMouseZoom;
            }
            else if (State == CameraMoveState.CameraMouseZoom)
            {
                if (Mathf.Abs(Input.mouseScrollDelta.y) < 0.0001f)
                    State = CameraMoveState.None;
            }

            if (State != CameraMoveState.None)
                PointerPosition = cam.ScreenToWorldPoint(Input.mousePosition);
        }

        private void HandleTouch()
        {
            if (ApplicationState.ControlMode.Value == ControllingMode.Items)
            {
                if (Input.touchCount == 1)
                {
                    var touch = Input.GetTouch(0);
                    if (touch.phase == TouchPhase.Began)
                    {
                        var screenPos = cam.ScreenToWorldPoint(touch.position);
                        if (IsPointerOverUI())
                            State = CameraMoveState.None;
                        else if (IsPointOverObject(screenPos))
                            State = CameraMoveState.WorkspaceOverItem;
                        else
                            State = CameraMoveState.WorkspaceClear;
                    }
                    else if (touch.phase == TouchPhase.Ended)
                        State = CameraMoveState.None;
                }
                else
                    State = CameraMoveState.None;
            }
            else
            {
                if (ApplicationState.ControlMode.Value == ControllingMode.CameraToggled)
                {
                    if (Input.touchCount == 1)
                    {
                        if (State != CameraMoveState.CameraMove)
                        {
                            var touch = Input.GetTouch(0);
                            if (touch.phase == TouchPhase.Began)
                            {
                                pointerStart = cam.ScreenToWorldPoint(touch.position);
                                if (!IsPointerOverUI())
                                    State = CameraMoveState.CameraMove;
                            }
                            else if (touch.phase == TouchPhase.Ended)
                                State = CameraMoveState.None;
                        }
                    }
                    else if (Input.touchCount == 2)
                    {
                        if (State != CameraMoveState.CameraPanAndZoom)
                        {
                            if (!IsTouchOverUI(0) && !IsTouchOverUI(1))
                            {
                                pointerStart = cam.ScreenToWorldPoint(GetTouchMiddle(0, 1));
                                prevTouchDistance = GetTouchDistance(0, 1);
                                State = CameraMoveState.CameraPanAndZoom;
                            }
                        }
                    }
                    else
                        State = CameraMoveState.None;
                }
                else
                {
                    if (Input.touchCount == 2)
                    {
                        if (State != CameraMoveState.CameraMove)
                        {
                            var touch = Input.GetTouch(1);
                            if (touch.phase == TouchPhase.Began)
                            {
                                pointerStart = cam.ScreenToWorldPoint(touch.position);
                                if (!IsTouchOverUI(1))
                                    State = CameraMoveState.CameraMove;
                            }
                            else if (touch.phase == TouchPhase.Ended)
                                State = CameraMoveState.None;
                        }
                    }
                    else if (Input.touchCount == 3)
                    {
                        if (State != CameraMoveState.CameraPanAndZoom)
                        {
                            if (!IsTouchOverUI(1) && !IsTouchOverUI(2))
                            {
                                pointerStart = cam.ScreenToWorldPoint(GetTouchMiddle(1, 2));
                                prevTouchDistance = GetTouchDistance(1, 2);
                                State = CameraMoveState.CameraPanAndZoom;
                            }
                        }
                    }
                    else
                        State = CameraMoveState.None;
                }
            }

			if (State == CameraMoveState.WorkspaceClear || State == CameraMoveState.WorkspaceOverItem)
				PointerPosition = cam.ScreenToWorldPoint(Input.GetTouch(0).position);

			if (State == CameraMoveState.CameraMove)
            {
                if (ApplicationState.ControlMode.Value == ControllingMode.CameraToggled)
                    PointerPosition = cam.ScreenToWorldPoint(Input.GetTouch(0).position);
                else
                    PointerPosition = cam.ScreenToWorldPoint(Input.GetTouch(1).position);
            }

            if (State == CameraMoveState.CameraPanAndZoom)
            {
                if (ApplicationState.ControlMode.Value == ControllingMode.CameraToggled)
                {
                    float distance = GetTouchDistance(0, 1);
                    touchDistanceDelta = distance - prevTouchDistance;
                    prevTouchDistance = distance;
                    PointerPosition = cam.ScreenToWorldPoint(GetTouchMiddle(0, 1));
                }
                else
                {
                    float distance = GetTouchDistance(1, 2);
                    touchDistanceDelta = distance - prevTouchDistance;
                    prevTouchDistance = distance;
                    PointerPosition = cam.ScreenToWorldPoint(GetTouchMiddle(1, 2));
                }
            }
        }

        static Vector2 GetTouchMiddle(int first, int second)
        {
            var touch1 = Input.GetTouch(first).position;
            var touch2 = Input.GetTouch(second).position;
            return touch1 + (touch2 - touch1) / 2.0f;
        }

        static float GetTouchDistance(int first, int second)
        {
            var touch1 = Input.GetTouch(first).position;
            var touch2 = Input.GetTouch(second).position;
            return Vector2.Distance(touch1, touch2);
        }

        private void HandleState()
        {

            switch (State)
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

        private void MoveCamera()
        {
            Vector2 delta = pointerStart - PointerPosition;
            transform.Translate(delta);
        }

        private void ZoomCamera()
        {
            var step = GetStep();
            var size = cam.orthographicSize + step;

            var before = WorldPoint();
            cam.orthographicSize = Mathf.Clamp(size, _minSize, _maxSize);
            Vector3 after = WorldPoint();

            transform.position = transform.position - (after - before);
        }

        private float GetStep()
        {
            if (!Application.isMobilePlatform)
                return -Input.mouseScrollDelta.y * _mouseZoomSpeed;

            if (ApplicationState.ControlMode.Value == ControllingMode.CameraToggled)
            {
                if (Input.touchCount == 2)
                    return -touchDistanceDelta * _touchZoomSpeed * cam.orthographicSize;
            }
            else
            {
                if (Input.touchCount == 3)
                    return -touchDistanceDelta * _touchZoomSpeed * cam.orthographicSize;
            }

            return 0.0f;
        }

        private Vector3 WorldPoint()
        {
            if (!Application.isMobilePlatform)
                return cam.ScreenToWorldPoint(Input.mousePosition);

            if (ApplicationState.ControlMode.Value == ControllingMode.CameraToggled)
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

        static bool IsPointOverObject(Vector2 screenPoint)
        {
            RaycastHit2D hit = Physics2D.Raycast(screenPoint, Vector2.zero);
            return hit.collider != null;
        }

        static bool IsPointerOverUI()
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

        static bool IsTouchOverUI(int t)
        {
            var touch = Input.GetTouch(t);
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
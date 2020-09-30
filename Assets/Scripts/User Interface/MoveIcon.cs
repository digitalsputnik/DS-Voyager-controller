using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VoyagerController;
using VoyagerController.Workspace;

namespace VoyagerApp.UI
{
    public class MoveIcon : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Color _pressedColor = Color.white;
        [SerializeField] private Color _releasedColor = Color.white;
        [Space(3)]
        [SerializeField] private Sprite _hand = null;
        [SerializeField] private Sprite _grab = null;

        private Image _image;
        private float _time;
        private ControllingMode _prevState;

        private void Start()
        {
            _image = GetComponent<Image>();
            _image.color = _releasedColor;

            ControllingModeChanged(ApplicationState.ControlMode.Value);

            SelectionMove.OnSelectionMoveStarted += SelectionMoveStarted;
            SelectionMove.OnSelectionMoveEnded += SelectionMoveEnded;
            ApplicationState.ColorWheelActive.OnChanged += ColorWheelActiveChanged;
            ApplicationState.ControlMode.OnChanged += ControllingModeChanged;
        }
        
        private void OnDestroy()
        {
            SelectionMove.OnSelectionMoveStarted -= SelectionMoveStarted;
            SelectionMove.OnSelectionMoveEnded -= SelectionMoveEnded;
            ApplicationState.ColorWheelActive.OnChanged -= ColorWheelActiveChanged;
            ApplicationState.ControlMode.OnChanged -= ControllingModeChanged;
        }

        private void ColorWheelActiveChanged(bool value)
        {
            gameObject.SetActive(!value);
        }

        private void ControllingModeChanged(ControllingMode value)
        {
            switch (value)
            {
                case ControllingMode.Items:
                    _image.sprite = _hand;
                    break;
                case ControllingMode.Camera:
                case ControllingMode.CameraToggled:
                    _image.sprite = _grab;
                    break;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                _prevState = ApplicationState.ControlMode.Value;
                ApplicationState.ControlMode.Value = ControllingMode.Camera;
            }

            if (Input.GetKeyUp(KeyCode.LeftAlt))
                ApplicationState.ControlMode.Value = _prevState;
        }

        private void SelectionMoveStarted()
        {
            gameObject.SetActive(false);
        }

        private void SelectionMoveEnded()
        {
            gameObject.SetActive(true);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _image.color = _pressedColor;
            _time = Time.time;

            if (ApplicationState.ControlMode.Value == ControllingMode.Items)
                ApplicationState.ControlMode.Value = ControllingMode.Camera;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Time.time - _time < 0.4f && ApplicationState.ControlMode.Value != ControllingMode.CameraToggled)
                ApplicationState.ControlMode.Value = ControllingMode.CameraToggled;
            else
                ApplicationState.ControlMode.Value = ControllingMode.Items;
            _image.color = _releasedColor;
        }
    }
}
using UnityEngine;
using VoyagerApp.Workspace;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VoyagerApp.UI
{
    public class MoveIcon : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] Color pressedColor = Color.white;
        [SerializeField] Color releasedColor = Color.white;
        [Space(3)]
        [SerializeField] Sprite hand = null;
        [SerializeField] Sprite grab = null;

        Image image;
        float time;
        ControllingMode prevState;

        void Start()
        {
            image = GetComponent<Image>();
            image.color = releasedColor;

            ControllingModeChanged(ApplicationState.ControllingMode.value);

            SelectionMove.onSelectionMoveStarted += SelectionMoveStarted;
            SelectionMove.onSelectionMoveEnded += SelectionMoveEnded;
            ApplicationState.ColorWheelActive.onChanged += ColorWheelActiveChanged;
            ApplicationState.ControllingMode.onChanged += ControllingModeChanged;
        }

        void ColorWheelActiveChanged(bool value)
        {
            gameObject.SetActive(!value);
        }

        void ControllingModeChanged(ControllingMode value)
        {
            if (value == ControllingMode.Items)
                image.sprite = hand;
            if (value == ControllingMode.Camera || value == ControllingMode.CameraToggled)
                image.sprite = grab;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                prevState = ApplicationState.ControllingMode.value;
                ApplicationState.ControllingMode.value = ControllingMode.Camera;
            }

            if (Input.GetKeyUp(KeyCode.LeftAlt))
                ApplicationState.ControllingMode.value = prevState;
        }

        void SelectionMoveStarted()
        {
            gameObject.SetActive(false);
        }

        void SelectionMoveEnded()
        {
            gameObject.SetActive(true);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            image.color = pressedColor;
            time = Time.time;

            if (ApplicationState.ControllingMode.value == ControllingMode.Items)
                ApplicationState.ControllingMode.value = ControllingMode.Camera;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Time.time - time < 0.4f && ApplicationState.ControllingMode.value != ControllingMode.CameraToggled)
                ApplicationState.ControllingMode.value = ControllingMode.CameraToggled;
            else
                ApplicationState.ControllingMode.value = ControllingMode.Items;

            image.color = releasedColor;
        }

        void OnDestroy()
        {
            SelectionMove.onSelectionMoveStarted -= SelectionMoveStarted;
            SelectionMove.onSelectionMoveEnded -= SelectionMoveEnded;
            ApplicationState.ColorWheelActive.onChanged -= ColorWheelActiveChanged;
            ApplicationState.ControllingMode.onChanged -= ControllingModeChanged;
        }
    }
}
using UnityEngine;
using VoyagerApp.Workspace;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VoyagerApp.UI
{
    public class MoveIcon : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public static bool pressed;
        public static bool onHold;

        [SerializeField] Color pressedColor = Color.white;
        [SerializeField] Color releasedColor = Color.white;
        [Space(3)]
        [SerializeField] Sprite hand = null;
        [SerializeField] Sprite grab = null;

        Image image;
        float time;

        void Start()
        {
            image = GetComponent<Image>();
            image.color = releasedColor;

            SelectionMove.onSelectionMoveStarted += SelectionMoveStarted;
            SelectionMove.onSelectionMoveEnded += SelectionMoveEnded;
            ApplicationState.ColorWheelActive.onChanged += ColorWheelActiveChanged;
        }

        void ColorWheelActiveChanged(bool value)
        {
            gameObject.SetActive(!value);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftAlt))
                OnPointerDown(null);

            if (Input.GetKeyUp(KeyCode.LeftAlt))
                OnPointerUp(null);
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
            image.color = pressedColor;
            image.sprite = grab;
            pressed = true;
            time = Time.time;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Time.time - time < 0.4f && !onHold)
            {
                onHold = true;
            }
            else
            {
                image.color = releasedColor;
                image.sprite = hand;
                pressed = false;
                onHold = false;
            }
        }

        void OnDestroy()
        {
            SelectionMove.onSelectionMoveStarted -= SelectionMoveStarted;
            SelectionMove.onSelectionMoveEnded -= SelectionMoveEnded;
            ApplicationState.ColorWheelActive.onChanged -= ColorWheelActiveChanged;
        }
    }
}
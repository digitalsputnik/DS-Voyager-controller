using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VoyagerApp.UI
{
    public class SetBoxSelectionMode : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] Color pressedColor = Color.white;
        [SerializeField] Color releasedColor = Color.white;
        [SerializeField] Image modeImage = null;
        [SerializeField] Sprite[] modes = null;

        SelectionState addPrev;
        SelectionState removePrev;

        void Start()
        {
            ApplicationState.ColorWheelActive.onChanged += ColorWheelActiveChanged;
            ApplicationState.SelectionMode.onChanged += SelectionStateChanged;
            ApplicationState.SelectionMode.value = SelectionState.Set;
        }

        void ColorWheelActiveChanged(bool value)
        {
            gameObject.SetActive(!value);
        }

        void Update()
        {
            if (!Application.isMobilePlatform)
            {
                if (!MoveIcon.pressed)
                {
                    if (Input.GetKeyDown(KeyCode.LeftShift))
                    {
                        addPrev = ApplicationState.SelectionMode.value;
                        ApplicationState.SelectionMode.value = SelectionState.Add;
                    }
                    else if (Input.GetKeyUp(KeyCode.LeftShift))
                        ApplicationState.SelectionMode.value = addPrev;

                    if (Input.GetKeyDown(KeyCode.LeftControl))
                    {
                        removePrev = ApplicationState.SelectionMode.value;
                        ApplicationState.SelectionMode.value = SelectionState.Remove;
                    }
                    else if (Input.GetKeyUp(KeyCode.LeftControl))
                        ApplicationState.SelectionMode.value = removePrev;
                }
            }
        }

        void OnDestroy()
        {
            ApplicationState.ColorWheelActive.onChanged -= ColorWheelActiveChanged;
            ApplicationState.SelectionMode.onChanged -= SelectionStateChanged;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            modeImage.color = pressedColor;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            int index = ((int)ApplicationState.SelectionMode.value) + 1;
            index = index % modes.Length;
            ApplicationState.SelectionMode.value = (SelectionState)index;
            modeImage.color = releasedColor;
        }

        void SelectionStateChanged(SelectionState value)
        {
            modeImage.sprite = modes[(int)value];
        }
    }
}
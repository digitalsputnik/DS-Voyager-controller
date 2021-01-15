using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VoyagerController.UI
{
    public class SelectionBoxMode : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Color _pressedColor = Color.white;
        [SerializeField] private Color _releasedColor = Color.white;
        [SerializeField] private Image _modeImage = null;
        [SerializeField] private Sprite[] _modeSprites = null;

        private SelectionMode _addPrev;
        private SelectionMode _removePrev;

        private void Start()
        {
            ApplicationState.ColorWheelActive.OnChanged += ColorWheelActiveChanged;
            ApplicationState.SelectMode.OnChanged += SelectionStateChanged;
            ApplicationState.SelectMode.Value = SelectionMode.Set;
        }

        private void Update()
        {
            if (Application.isMobilePlatform) return;
            if (ApplicationState.ControlMode.Value != ControllingMode.Items) return;
            
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                _addPrev = ApplicationState.SelectMode.Value;
                ApplicationState.SelectMode.Value = SelectionMode.Add;
            }
            else if (Input.GetKeyUp(KeyCode.LeftShift))
                ApplicationState.SelectMode.Value = _addPrev;

            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                _removePrev = ApplicationState.SelectMode.Value;
                ApplicationState.SelectMode.Value = SelectionMode.Remove;
            }
            else if (Input.GetKeyUp(KeyCode.LeftControl))
                ApplicationState.SelectMode.Value = _removePrev;
        }

        private void OnDestroy()
        {
            ApplicationState.ColorWheelActive.OnChanged -= ColorWheelActiveChanged;
            ApplicationState.SelectMode.OnChanged -= SelectionStateChanged;
        }

        private void ColorWheelActiveChanged(bool value)
        {
            gameObject.SetActive(!value);
        }

        private void SelectionStateChanged(SelectionMode value)
        {
            _modeImage.sprite = _modeSprites[(int)value];
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _modeImage.color = _pressedColor;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            var index = (int)ApplicationState.SelectMode.Value + 1;
            index %= _modeSprites.Length;
            ApplicationState.SelectMode.Value = (SelectionMode)index;
            _modeImage.color = _releasedColor;
        }
    }
}
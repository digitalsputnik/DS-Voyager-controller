using UnityEngine;
using UnityEngine.UI;

namespace VoyagerApp.UI.Menus
{
    public class SliderMenu : Menu
    {
        public static SliderMenu instance;
        void Awake() => instance = this;

        public Slider slider;
        public Text titleText;
        public InputField valueInputField;

        IntField toModify;

        public override void Start()
        {
            base.Start();

            var rect = GetComponent<RectTransform>();
            rect.offsetMin = new Vector2(0.0f, 0.0f);
            rect.offsetMax = new Vector2(0.0f, 0.0f);

            slider.onValueChanged.AddListener(ValueChanged);
            valueInputField.onValueChanged.AddListener(InputChanged);
            valueInputField.onEndEdit.AddListener(ExitEdit);
        }

        void OnDestroy()
        {
            slider.onValueChanged.RemoveListener(ValueChanged);
            valueInputField.onValueChanged.RemoveListener(InputChanged);
            valueInputField.onEndEdit.RemoveListener(ExitEdit);
        }

        public void Use(IntField field)
        {
            titleText.text = field.gameObject.name.ToUpper();
            toModify = field;
            slider.value = field.normalized;
            Open = true;
        }

        public void Close()
        {
            toModify = null;
            Open = false;
        }

        void ValueChanged(float value)
        {
            if (toModify != null)
            {
                toModify.SetValue(value);

                valueInputField.onValueChanged.RemoveListener(InputChanged);
                valueInputField.text = toModify.Value.ToString();
                valueInputField.onValueChanged.AddListener(InputChanged);
            }
        }

        void InputChanged(string text)
        {
            if (text == string.Empty) return;

            if (toModify != null)
            {
                int value = int.Parse(text);
                toModify.SetValue(value);
                slider.onValueChanged.RemoveListener(ValueChanged);
                slider.normalizedValue = toModify.normalized;
                slider.onValueChanged.AddListener(ValueChanged);
            }
        }

        void ExitEdit(string text)
        {
            if (toModify != null)
            {
                int value = int.Parse(text);
                if (value > toModify.max || value < toModify.min)
                {
                    value = Mathf.Clamp(value, toModify.min, toModify.max);
                    valueInputField.text = value.ToString();
                    return;
                }
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

namespace VoyagerApp.UI.Menus
{
    public class SliderMenu : Menu
    {
        public static SliderMenu instance;
        void Awake() => instance = this;

        public Slider slider;
        public Text text;
        public Text valueText;

        IntField toModify;

        public override void Start()
        {
            base.Start();

            var rect = GetComponent<RectTransform>();
            rect.offsetMin = new Vector2(0.0f, 0.0f);
            rect.offsetMax = new Vector2(0.0f, 0.0f);

            slider.onValueChanged.AddListener(ValueChanged);
        }

        void OnDestroy()
        {
            slider.onValueChanged.RemoveListener(ValueChanged);
        }

        public void Use(IntField field)
        {
            text.text = field.gameObject.name.ToUpper();
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
                valueText.text = toModify.Value.ToString();
            }
        }
    }
}
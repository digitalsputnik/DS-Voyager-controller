using UnityEngine;
using UnityEngine.UI;

namespace VoyagerApp.UI.Menus
{
    public class IntField : MonoBehaviour
    {
        public event FieldChangedHandler onChanged;

        [SerializeField] InputField field   = null;
        [SerializeField] Button increase    = null;
        [SerializeField] Button decrease    = null;
        [SerializeField] int min            = 0;
        [SerializeField] int max            = 100;
        [SerializeField] int startValue     = 50;

        public int Value => int.Parse(field.text);
        public float normalized => (float)(Value - min) / (max - min);

        void Start()
        {
            field.onValueChanged.AddListener(FieldChanged);
            increase.onClick.AddListener(Increase);
            decrease.onClick.AddListener(Decrease);
            SetValue(startValue);
        }

        void FieldChanged(string text)
        {
            if (text != "")
            {
                int value = int.Parse(text);
                SetValue(value);
            }
        }

        void Increase()
        {
            SetValue(Value + 1);
        }

        void Decrease()
        {
            SetValue(Value - 1);
        }

        public void SetValue(int value)
        {
            value = Mathf.Clamp(value, min, max);
            field.text = value.ToString();
            onChanged?.Invoke(value);
        }

        public void SetValue(float value)
        {
            int val = (int)((max - min) * value) + min;
            SetValue(val);
        }

        public void Expand()
        {
            SliderMenu.instance.Use(this);
        }
    }

    public delegate void FieldChangedHandler(int value);
}
using UnityEngine;
using UnityEngine.UI;

namespace VoyagerApp.UI.Menus
{
    public class IntField : MonoBehaviour
    {
        public event FieldChangedHandler onChanged;

        [SerializeField] Text valueText     = null;
        [SerializeField] Button expandBtn   = null;
        public int min                      = 0;
        public int max                      = 100;
        [SerializeField] int startValue     = 50;

        public int Value => int.Parse(valueText.text);
        public float normalized => (float)(Value - min) / (max - min);

        void Start()
        {
            SetValue(startValue);
            expandBtn.onClick.AddListener(Expand);
        }

        void Expand() => SliderMenu.instance.Use(this);

        public void SetValue(int value)
        {
            value = Mathf.Clamp(value, min, max);
            valueText.text = value.ToString();
            onChanged?.Invoke(value);
        }

        public void SetValue(float value)
        {
            int val = (int)((max - min) * value) + min;
            SetValue(val);
        }
    }

    public delegate void FieldChangedHandler(int value);
}
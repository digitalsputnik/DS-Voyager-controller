using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace VoyagerController.UI
{
    public class IntField : MonoBehaviour
    {
        public event FieldChangedHandler OnChanged;

        public int Min = 0;
        public int Max = 100;
        public string PresetSuffix = "";
        public float[] Presets = null;
        
        [Space(3)]
        [SerializeField] private Text _valueText = null;
        [SerializeField] private Button _expandBtn = null;
        [SerializeField] private int _startValue = 50;

        public int Value => int.Parse(_valueText.text);
        
        public float Normalized => (float)(Value - Min) / (Max - Min);

        public bool Interactable
        {
            get => _expandBtn.interactable;
            set => _expandBtn.interactable = value;
        }

        private void Start()
        {
            SetValue(_startValue);
            _expandBtn.onClick.AddListener(Expand);
        }

        private void Expand() => SliderMenu.Use(this);

        public void SetValue(int value)
        {
            value = Mathf.Clamp(value, Min, Max);
            _valueText.text = value.ToString();
            OnChanged?.Invoke(value);
        }

        public void SetValue(float value)
        {
            var actualValue = ((Max - Min) * value) + Min;
            var val = (int)math.round(actualValue);
            SetValue(val);
        }
    }

    public delegate void FieldChangedHandler(int value);
}
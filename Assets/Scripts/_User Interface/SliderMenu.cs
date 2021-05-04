using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace VoyagerController.UI
{
    public class SliderMenu : Menu
    {
        private static SliderMenu _instance;
        private void Awake() => _instance = this;

        [SerializeField] private Transform _presetsContainer = null;
        [SerializeField] private Button _presetButton = null;
        [SerializeField] private Slider _slider = null;
        [SerializeField] private Text _titleText = null;
        [SerializeField] private InputField _valueInputField = null;

        private readonly List<Button> _presets = new List<Button>();
        private List<float> _presetValues = new List<float>();

        private IntField _target;
        private int _startValue;

        public override void Start()
        {
            base.Start();

            var rect = GetComponent<RectTransform>();
            rect.offsetMin = new Vector2(0.0f, 0.0f);
            rect.offsetMax = new Vector2(0.0f, 0.0f);

            _slider.onValueChanged.AddListener(ValueChanged);
            _valueInputField.onValueChanged.AddListener(InputChanged);
            _valueInputField.onEndEdit.AddListener(ExitEdit);
        }

        void OnDestroy()
        {
            _slider.onValueChanged.RemoveListener(ValueChanged);
            _valueInputField.onValueChanged.RemoveListener(InputChanged);
            _valueInputField.onEndEdit.RemoveListener(ExitEdit);
        }

        public static void Use(IntField field)
        {
            _instance._titleText.text = field.gameObject.name.ToUpper();
            _instance._target = field;
            _instance._slider.value = field.Normalized;
            _instance._valueInputField.text = field.Value.ToString();

            _instance._startValue = field.Value;

            if (field.Presets.Length != 0)
            {
                _instance._presetValues = field.Presets.ToList();
                _instance.DrawPresets();
            }

            _instance.Open = true;
        }

        public void Close()
        {
            _target = null;
            Open = false;
            ClearPresets();
        }

        public void Cancel()
        {
            _target.SetValue(_startValue);
            _target = null;
            Open = false;
            ClearPresets();
        }

        private void DrawPresets()
        {
            var i = 0;
            foreach (var value in _presetValues)
            {
                var index = i;
                var minmax = (int)math.round(value * (_target.Max - _target.Min) + _target.Min);
                var preset = Instantiate(_presetButton, _presetsContainer);
                preset.GetComponentInChildren<Text>().text = $"{minmax}{_target.PresetSuffix}";
                preset.GetComponent<RectTransform>().sizeDelta = new float2(300.0f, 120.0f);
                preset.onClick.AddListener(() => OnPresetClicked(index));
                _presets.Add(preset);
                i++;
            }
            Canvas.ForceUpdateCanvases();
        }

        private void ClearPresets()
        {
            foreach (var preset in _presets.ToList())
            {
                _presets.Remove(preset);
                Destroy(preset.gameObject);
            }
        }

        private void OnPresetClicked(int index)
        {
            var value = _presetValues[index];
            _slider.normalizedValue = value;
        }

        private void ValueChanged(float value)
        {
            if (_target == null) return;
            
            _target.SetValue(value);
            _valueInputField.onValueChanged.RemoveListener(InputChanged);
            _valueInputField.text = _target.Value.ToString();
            _valueInputField.onValueChanged.AddListener(InputChanged);
        }

        private void InputChanged(string text)
        {
            if (text == string.Empty) return;
            if (_target == null) return;
            
            var value = int.Parse(text);
            _target.SetValue(value);
            _slider.onValueChanged.RemoveListener(ValueChanged);
            _slider.normalizedValue = _target.Normalized;
            _slider.onValueChanged.AddListener(ValueChanged);
        }

        private void ExitEdit(string text)
        {
            var value = int.Parse(text);

            if (_target == null) return;
            if (value <= _target.Max && value >= _target.Min) return;
            
            value = Mathf.Clamp(value, _target.Min, _target.Max);
            _valueInputField.text = value.ToString();
        }
    }
}
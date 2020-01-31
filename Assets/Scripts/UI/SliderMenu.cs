using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace VoyagerApp.UI.Menus
{
    public class SliderMenu : Menu
    {
        public static SliderMenu instance;
        void Awake() => instance = this;

        public Transform presetsContainer = null;
        public Button presetButton        = null;
        public Slider slider              = null;
        public Text titleText             = null;
        public InputField valueInputField = null;

        IntField toModify;

        List<Button> presets = new List<Button>();
        List<float> presetValues = new List<float>();

        int startValue;

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
            valueInputField.text = field.Value.ToString();

            startValue = field.Value;

            if (field.presets.Length != 0)
            {
                presetValues = field.presets.ToList();
                DrawPresets();
            }

            Open = true;
        }

        public void Close()
        {
            toModify = null;
            Open = false;
            ClearPresets();
        }

        public void Cancel()
        {
            toModify.SetValue(startValue);
            toModify = null;
            Open = false;
            ClearPresets();
        }

        void DrawPresets()
        {
            int i = 0;
            foreach (var value in presetValues)
            {
                int index = i;
                int minmax = (int)math.round(value * (toModify.max - toModify.min) + toModify.min);
                Button preset = Instantiate(presetButton, presetsContainer);
                preset.GetComponentInChildren<Text>().text = $"{minmax}{toModify.presetSuffix}";
                preset.GetComponent<RectTransform>().sizeDelta = new float2(300.0f, 120.0f);
                preset.onClick.AddListener(() => OnPresetClicked(index));
                presets.Add(preset);
                i++;
            }
            Canvas.ForceUpdateCanvases();
        }

        void ClearPresets()
        {
            foreach (var preset in presets.ToList())
            {
                presets.Remove(preset);
                Destroy(preset.gameObject);
            }
        }

        void OnPresetClicked(int index)
        {
            Debug.Log(index);
            float value = presetValues[index];
            slider.normalizedValue = value;
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
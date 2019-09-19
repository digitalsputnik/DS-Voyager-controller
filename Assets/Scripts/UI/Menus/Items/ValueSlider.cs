using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VoyagerApp.UI.Menus
{
    public class ValueSlider : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public int min;
        public int max;
        public int value;
        public float timer = 0.6f;
        [Space(3)]
        [SerializeField] Text valueText = null;
        [SerializeField] Image fillImage = null;
        [SerializeField] RectTransform controlArea = null;
        [SerializeField] InputField field = null;
        public bool isTemperature;

        public Slider[] sliderObject;
        [Space(3)]

        public GameObject[] sliderText;

        public ValueSliderEvent onChanged;

        public float normalized;
        float stepVal;

        [Range(1f, 5f)]
        public int stepMultiplier = 1;
        private float updateSpeed = 0.1f;

        void Start()
        {
            normalized = (float)(value - min) / (max - min);
            valueText.text = value.ToString();
            foreach (Slider sO in sliderObject)
                sO.value = normalized;
        }

        public void ChangeValueOnHold(float position)
        {
            int range = max - min;
            stepVal = range / 100;
            ChangeValue(
                stepVal,
                stepVal * -1,
                position);
        }

        public void SliderUpdate()
        {
            foreach (Slider sO in sliderObject)
                sO.value = normalized;
        }

        public void ChangeValue(float posVal, float negVal, float pos)
        {
            int increasement = pos > 0.5 ? (int)posVal : (int)negVal;
            SetValue(Mathf.Clamp(value + increasement, min, max));
        }

        SliderState state;
        bool pointerDown;
        float updateTime;
        float startPosition;

        enum SliderState
        {
            Click,
            Slide,
            Hold
        }

		public void OnPointerDown(PointerEventData eventData)
		{
            state = SliderState.Click;
            updateTime = Time.time;
            pointerDown = true;
            startPosition = HorizontalValueFromPosition(eventData.position);
        }            

        void Update()
        {
            if (!pointerDown) return;

            float passed = Time.time - updateTime;
            if (state == SliderState.Click && passed > timer)
                state = SliderState.Hold;

            passed = Time.time - updateSpeed;
            if (state == SliderState.Hold && passed > timer)
            {
                ChangeValueOnHold(startPosition);
                updateTime = Time.time;
            }
        }

        //public void OnDrag(PointerEventData eventData)
        //{
        //    if (state == SliderState.Click)
        //        state = SliderState.Slide;

        //    if (state == SliderState.Slide)
        //    {
        //        float val = HorizontalValueFromPosition(eventData.position);
        //        SetValue(val);
        //    }
        //}


        public void textInput()
        {
            value = int.Parse(field.text);
            SetValue(value);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (state == SliderState.Click)
            {
                float pos = startPosition;
                ChangeValue(1, -1, pos);
            }

            pointerDown = false;
        }

        float HorizontalValueFromPosition(Vector2 position)
        {
            Vector3[] corners = new Vector3[4];
            controlArea.GetLocalCorners(corners);

            float left = transform.position.x + corners[0].x;
            float right = transform.position.x + corners[2].x;
            float point = position.x - left;

            return Mathf.Clamp01(point / (right - left));
        }

        public void SetValue(int val)
        {
            if (val != value)
            {
                value = val;
                normalized = (float)(value - min) / (max - min);
//                field.text = value.ToString();
                UpdateUI(normalized);
                var eventData = new ValueSliderEventData(min, max, value, normalized);
                onChanged.Invoke(eventData);
            }
        }

        public void SetValue(float val)
        {
            //Different calculation for Temperature
            if (isTemperature)
            {
                int iVal = (int)((max - min) * val) + min;
                iVal /= 100;
                SetValue(iVal * 100);
            }
            else
            { 
                int iVal = (int)(((max - min) * val) + min);
                SetValue(iVal);
            }

            normalized = val;
        }

        void UpdateUI(float fill)
        {
            //fillImage.fillAmount = fill;
            valueText.text = value.ToString();
            foreach (GameObject sText in sliderText)
                sText.GetComponent<Text>().text = value.ToString();
        }
    }

    [Serializable]
    public class ValueSliderEvent : UnityEvent<ValueSliderEventData> { }

    [Serializable]
    public struct ValueSliderEventData
    {
        public int min;
        public int max;
        public int value;
        public float clampedValue;

        public ValueSliderEventData(int min, int max, int value, float clamped)
        {
            this.min = min;
            this.max = max;
            this.value = value;
            this.clampedValue = clamped;
        }
    }
}
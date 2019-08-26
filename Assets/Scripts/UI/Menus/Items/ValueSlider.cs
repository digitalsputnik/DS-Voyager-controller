using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;

namespace VoyagerApp.UI.Menus
{
    public class ValueSlider : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        public int min;
        public int max;
        public int value;
        public float timer = 0.6f;
        [Space(3)]
        [SerializeField] Text valueText;
        [SerializeField] Image fillImage;
        [SerializeField] RectTransform controlArea;
        [Space(3)]
        [SerializeField] int intensityStep = 1;
        [SerializeField] int temperatureStep = 10;
        [SerializeField] int saturationStep = 1;
        [SerializeField] int hueStep = 3;

        string sliderName;
        [Space(3)]
        public ValueSliderEvent onChanged;
        public float normalized => (float)(value - min) / (max - min);
		bool mouseHold;
        bool mouseDrag;
        bool buttonUp;


        float val;
        float update;
        private float updateSpeed = 0.03f;

        IEnumerator waitForHold()
        {
            yield return new WaitForSeconds(timer);
            if(buttonUp != true)
            mouseHold = true;
        }

        void Start()
        {
            valueText.text = value.ToString();
            update = 0.0f;
           
        }

		void Update()
        {
            //Update speed
            update += Time.deltaTime;
            if (update > updateSpeed)
            {
                update = 0.0f;
                // Increases or decreases value once per update when enabled
                if (mouseHold == true && mouseDrag == false)
                    changeValueOnHold();
            }
		}

        public void changeValueOnHold()
        { 
            switch(sliderName)
            {   
                case "Itensity":
                    changeValue(intensityStep, intensityStep * -1);
                    break;
                case "Temperature":
                    changeValue(temperatureStep, temperatureStep * -1);
                    break;
                case "Saturation":
                    changeValue(saturationStep, saturationStep * -1);
                    break;
                case "Hue":
                    changeValue(hueStep, hueStep * -1);
                    break;
                default:
                    changeValue(1, -1);
                    break;
            }
        }


        public void changeValue(int posVal, int negVal)
        {
            int increasement = val > 0.5 ? posVal : negVal;
            SetValue(Mathf.Clamp(value + increasement, min, max));
        }

		public void OnPointerDown(PointerEventData eventData)
		{

            val = HorizontalValueFromPosition(eventData.position);
            changeValue(1, -1); // Increases or decreases value once per click
            sliderName = name;
            buttonUp = false;
            StartCoroutine("waitForHold"); // Delay before update
		}

		public void OnPointerUp(PointerEventData pointerEventData)
		{
            buttonUp = true; // Prevents coroutine from activating
            sliderName = null;
            //Resets booleans
            mouseHold = false;
            mouseDrag = false;
		}

		public void OnDrag(PointerEventData eventData)
        {
            mouseDrag = true; // Prevents value changing during drag

            val = HorizontalValueFromPosition(eventData.position);
            int iVal = (int)((max - min) * val + min);
            SetValue(iVal);
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
                float fill = (float)(value - min) / (max - min);
                UpdateUI(fill);
                var eventData = new ValueSliderEventData(min, max, value, fill);
                onChanged.Invoke(eventData);
            }
        }

        public void SetValue(float val)
        {
            int iVal = (int)((max - min) * val);
            SetValue(iVal);
        }

        void UpdateUI(float fill)
        {
            fillImage.fillAmount = fill;
            valueText.text = value.ToString();
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
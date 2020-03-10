using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.UI.Overlays;

namespace VoyagerApp.UI.Menus
{
    public class ColorWheelMenu : Menu
    {
        [SerializeField] CanvasGroup colorWheelCanvasGroup  = null;
        [SerializeField] ShowHideMenu showHide              = null;
        [SerializeField] GameObject controlsContainer       = null;
        [Space(3)]
        [SerializeField] IntField itensitySlider            = null;
        [SerializeField] IntField temperatureSlider         = null;
        [SerializeField] IntField saturationSlider          = null;
        [SerializeField] IntField hueSlider                 = null;
        [SerializeField] IntField effectSlider              = null;
        [Space(3)]
        [SerializeField] ColorWheelEventSystem wheel        = null;

        Itshe beginning;
        Itshe itshe;

        bool approved;

        public void SetItsh(Itshe itshe)
        {
            beginning = itshe;
            this.itshe = itshe;

            itensitySlider.SetValue(itshe.i);
            temperatureSlider.SetValue(itshe.t);
            saturationSlider.SetValue(itshe.s);
            hueSlider.SetValue(itshe.h);
            effectSlider.SetValue(itshe.e);

            wheel.SetFromItsh(itshe);
        }

        public void Approve()
        {
            approved = true;
            GetComponentInParent<InspectorMenuContainer>().ShowMenu(null);
        }

        public override void Start()
        {
            base.Start();

            var rect = colorWheelCanvasGroup.GetComponent<RectTransform>();
            rect.offsetMin = new Vector2(0.0f, 0.0f);
            rect.offsetMax = new Vector2(0.0f, 0.0f);

            HideColorWheel();
        }

        internal override void OnShow()
        {
            ShowColorWheel();
            SubscribeSliders();
            SubscribeWheel();

            InvokeRepeating("UpdateLoop", 0.0f, 0.2f);
            ApplicationState.ColorWheelActive.value = true;
            showHide.GetComponent<Button>().onClick.AddListener(Approve);
        }

        internal override void OnHide()
        {
            HideColorWheel();
            UnsubscribeSliders();
            UnsubscribeWheel();

            StopAllCoroutines();
            ApplicationState.ColorWheelActive.value = false;
            showHide.GetComponent<Button>().onClick.RemoveListener(Approve);
            DialogBox.ResumeDialogues();
        }

        Itshe prevItshe;

        void UpdateLoop()
        {
            if (itshe != prevItshe)
            {
                ColorwheelManager.instance.ValuePicked(itshe);
                prevItshe = itshe;
            }
        }

        #region Sliders

        void SubscribeSliders()
        {
            itensitySlider.onChanged    += SliderChanged;
            temperatureSlider.onChanged += SliderChanged;
            saturationSlider.onChanged  += SliderChanged;
            hueSlider.onChanged         += SliderChanged;
            effectSlider.onChanged      += SliderChanged;
        }

        void UnsubscribeSliders()
        {
            itensitySlider.onChanged    -= SliderChanged;
            temperatureSlider.onChanged -= SliderChanged;
            saturationSlider.onChanged  -= SliderChanged;
            hueSlider.onChanged         -= SliderChanged;
            effectSlider.onChanged      -= SliderChanged;
        }

        void SliderChanged(int value)
        {
            float i = itensitySlider.normalized;
            float t = temperatureSlider.normalized;
            float s = saturationSlider.normalized;
            float h = hueSlider.normalized;
            float e = effectSlider.normalized;

            itshe = new Itshe(i, t, s, h, e);

            UnsubscribeWheel();
            wheel.SetFromItsh(itshe);
            SubscribeWheel();
        }

        #endregion

        #region Wheel

        void SubscribeWheel()
        {
            wheel.onHueSaturationChanged += WheelHueSaturationChanged;
        }

        void UnsubscribeWheel()
        {
            wheel.onHueSaturationChanged -= WheelHueSaturationChanged;
        }

        private void WheelHueSaturationChanged(float hue, float saturation)
        {
            itshe.s = saturation;
            itshe.h = hue;

            UnsubscribeSliders();
            saturationSlider.SetValue(saturation);
            hueSlider.SetValue(hue);
            SubscribeSliders();
        }

        #endregion

        void Update()
        {
            if (!Open) return;

            if (!showHide.Open && colorWheelCanvasGroup.interactable)
                HideColorWheel();

            if (showHide.Open && !colorWheelCanvasGroup.interactable)
                ShowColorWheel();
        }

        void ShowColorWheel()
        {
            colorWheelCanvasGroup.alpha = 1.0f;
            colorWheelCanvasGroup.interactable = true;
            colorWheelCanvasGroup.blocksRaycasts = true;

            controlsContainer.SetActive(false);

            approved = false;
        }

        void HideColorWheel()
        {
            colorWheelCanvasGroup.alpha = 0.0f;
            colorWheelCanvasGroup.interactable = false;
            colorWheelCanvasGroup.blocksRaycasts = false;

            controlsContainer.SetActive(true);

            if (!approved)
                ColorwheelManager.instance.ValuePicked(beginning);
        }
    }
}
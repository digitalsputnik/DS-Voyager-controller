using UnityEngine;
using VoyagerApp.Workspace;

namespace VoyagerApp.UI.Menus
{
    public class ColorWheelMenu : Menu
    {
        [SerializeField] CanvasGroup colorWheelCanvasGroup  = null;
        [SerializeField] ShowHideMenu showHide              = null;
        [SerializeField] GameObject controlsContainer       = null;
        [Space(3)]
        [SerializeField] ValueSlider itensitySlider         = null;
        [SerializeField] ValueSlider temperatureSlider      = null;
        [SerializeField] ValueSlider saturationSlider       = null;
        [SerializeField] ValueSlider hueSlider              = null;
        [Space(3)]
        [SerializeField] ColorWheelEventSystem wheel        = null;

        Itsh beginning;
        Itsh itsh;

        bool approved;

        public void SetItsh(Itsh itsh)
        {
            beginning = itsh;
            this.itsh = itsh;

            itensitySlider.SetValue(itsh.i);
            temperatureSlider.SetValue(itsh.t);
            saturationSlider.SetValue(itsh.s);
            hueSlider.SetValue(itsh.h);

            wheel.SetFromItsh(itsh);
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
        }

        internal override void OnHide()
        {
            HideColorWheel();
            UnsubscribeSliders();
            UnsubscribeWheel();
        }

        #region Sliders

        void SubscribeSliders()
        {
            itensitySlider.onChanged.AddListener(SliderChanged);
            temperatureSlider.onChanged.AddListener(SliderChanged);
            saturationSlider.onChanged.AddListener(SliderChanged);
            hueSlider.onChanged.AddListener(SliderChanged);
        }

        void UnsubscribeSliders()
        {
            itensitySlider.onChanged.RemoveListener(SliderChanged);
            temperatureSlider.onChanged.RemoveListener(SliderChanged);
            saturationSlider.onChanged.RemoveListener(SliderChanged);
            hueSlider.onChanged.RemoveListener(SliderChanged);
        }

        void SliderChanged(ValueSliderEventData data)
        {
            float i = itensitySlider.normalized;
            float t = temperatureSlider.normalized;
            float s = saturationSlider.normalized;
            float h = hueSlider.normalized;
            itsh = new Itsh(i, t, s, h);

            ColorwheelManager.instance.ValuePicked(itsh);

            UnsubscribeWheel();
            wheel.SetFromItsh(itsh);
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
            itsh.s = saturation;
            itsh.h = hue;

            ColorwheelManager.instance.ValuePicked(itsh);

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

            ItemMove.Enabled = false;
        }

        void HideColorWheel()
        {
            colorWheelCanvasGroup.alpha = 0.0f;
            colorWheelCanvasGroup.interactable = false;
            colorWheelCanvasGroup.blocksRaycasts = false;

            controlsContainer.SetActive(true);

            if (!approved)
                ColorwheelManager.instance.ValuePicked(beginning);

            ItemMove.Enabled = true;
        }
    }
}
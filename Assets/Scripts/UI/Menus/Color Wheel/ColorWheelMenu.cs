using UnityEngine;
using VoyagerApp.Workspace;
using UnityEngine.UI;

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
        [SerializeField] ValueSlider effectSlider           = null;
        [Space(3)]
        [SerializeField] ColorWheelEventSystem wheel        = null;

        Itshe beginning;
        Itshe itshe;

        bool approved;
        bool selectionEnabled;

        public void SetItsh(Itshe itshe)
        {
            Debug.Log($"Itsh on open {itshe}");
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
            Debug.Log($"Itsh on keep {itshe}");
            approved = true;
            GameObject.Find("Minimize / Maximize").GetComponent<Button>().enabled = true;
            GameObject.Find("CWSliderToggle").SetActive(false);
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
            effectSlider.onChanged.AddListener(SliderChanged);
        }

        void UnsubscribeSliders()
        {
            itensitySlider.onChanged.RemoveListener(SliderChanged);
            temperatureSlider.onChanged.RemoveListener(SliderChanged);
            saturationSlider.onChanged.RemoveListener(SliderChanged);
            hueSlider.onChanged.RemoveListener(SliderChanged);
            effectSlider.onChanged.RemoveListener(SliderChanged);
        }

        void SliderChanged(ValueSliderEventData data)
        {
            float i = itensitySlider.normalized;
            float t = temperatureSlider.normalized;
            float s = saturationSlider.normalized;
            float h = hueSlider.normalized;
            float e = effectSlider.normalized;

            itshe = new Itshe(i, t, s, h, e);

            ColorwheelManager.instance.ValuePicked(itshe);

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

            ColorwheelManager.instance.ValuePicked(itshe);

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
            selectionEnabled = WorkspaceSelection.instance.Enabled;

            ItemMove.Enabled = false;
            WorkspaceSelection.instance.Enabled = false;
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
            WorkspaceSelection.instance.Enabled = selectionEnabled;
        }
    }
}
using DigitalSputnik.Colors;
using UnityEngine;

namespace VoyagerController.UI
{
    public class ColorWheelSettings : Menu
    {
        [SerializeField] private CanvasGroup _colorWheelCanvasGroup = null;
        [SerializeField] private GameObject[] _sideMenuContainers = null;
        [Space(3)]
        [SerializeField] private IntField _intensitySlider = null;
        [SerializeField] private IntField _temperatureSlider = null;
        [SerializeField] private IntField _saturationSlider = null;
        [SerializeField] private IntField _hueSlider = null;
        [Space(3)]
        [SerializeField] private ColorWheelEventSystem _wheel = null;

        private Itshe _beginning;
        private Itshe _itshe;
        private bool _approved;
        private bool _changed;

        public void SetItsh(Itshe itshe)
        {
            _beginning = new Itshe(itshe.I, itshe.T, itshe.S, itshe.H, itshe.E);
            _itshe = itshe;

            _intensitySlider.SetValue(itshe.I);
            _temperatureSlider.SetValue(itshe.T);
            _saturationSlider.SetValue(itshe.S);
            _hueSlider.SetValue(itshe.H);

            _wheel.SetFromItsh(itshe);
        }

        public void Approve()
        {
            _approved = true;
        }

        public override void Start()
        {
            base.Start();

            var rect = _colorWheelCanvasGroup.GetComponent<RectTransform>();
            rect.offsetMin = new Vector2(0.0f, 0.0f);
            rect.offsetMax = new Vector2(0.0f, 0.0f);

            HideColorWheel();
        }

        internal override void OnShow()
        {
            ShowColorWheel();
            SubscribeSliders();
            SubscribeWheel();

            InvokeRepeating(nameof(UpdateLoop), 0.0f, 0.2f);
        }

        internal override void OnHide()
        {
            HideColorWheel();
            UnsubscribeSliders();
            UnsubscribeWheel();

            StopAllCoroutines();
        }

        private void UpdateLoop()
        {
            if (!_changed) return;

            ColorWheelManager.ValuePicked(_itshe);
            _changed = false;
        }

        #region Sliders

        private void SubscribeSliders()
        {
            _intensitySlider.OnChanged += SliderChanged;
            _temperatureSlider.OnChanged += SliderChanged;
            _saturationSlider.OnChanged += SliderChanged;
            _hueSlider.OnChanged += SliderChanged;
        }

        private void UnsubscribeSliders()
        {
            _intensitySlider.OnChanged -= SliderChanged;
            _temperatureSlider.OnChanged -= SliderChanged;
            _saturationSlider.OnChanged -= SliderChanged;
            _hueSlider.OnChanged -= SliderChanged;
        }

        private void SliderChanged(int value)
        {
            var i = _intensitySlider.Normalized;
            var t = _temperatureSlider.Normalized;
            var s = _saturationSlider.Normalized;
            var h = _hueSlider.Normalized;

            _itshe = new Itshe(i, t, s, h, 1.0f);

            UnsubscribeWheel();
            _wheel.SetFromItsh(_itshe);
            SubscribeWheel();

            _changed = true;
        }

        #endregion

        #region Wheel

        private void SubscribeWheel()
        {
            _wheel.OnHueSaturationChanged += WheelHueSaturationChanged;
        }

        private void UnsubscribeWheel()
        {
            _wheel.OnHueSaturationChanged -= WheelHueSaturationChanged;
        }

        private void WheelHueSaturationChanged(float hue, float saturation)
        {
            _itshe.S = saturation;
            _itshe.H = hue;

            UnsubscribeSliders();
            _saturationSlider.SetValue(saturation);
            _hueSlider.SetValue(hue);
            SubscribeSliders();

            _changed = true;
        }
        #endregion

        private void ShowColorWheel()
        {
            _colorWheelCanvasGroup.alpha = 1.0f;
            _colorWheelCanvasGroup.interactable = true;
            _colorWheelCanvasGroup.blocksRaycasts = true;

            foreach (var container in _sideMenuContainers)
                container.SetActive(false);

            _approved = false;
        }

        public void HideMenu()
        {
            Open = false;
        }

        public void HideColorWheel()
        {
            _colorWheelCanvasGroup.alpha = 0.0f;
            _colorWheelCanvasGroup.interactable = false;
            _colorWheelCanvasGroup.blocksRaycasts = false;

            foreach (var container in _sideMenuContainers)
                container.SetActive(true);

            if (!_approved)
                ColorWheelManager.ValuePicked(_beginning);
        }
    }
}
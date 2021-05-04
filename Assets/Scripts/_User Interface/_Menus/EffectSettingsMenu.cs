using UnityEngine;
using VoyagerController.Effects;
using VoyagerController.Mapping;
using VoyagerController.Workspace;

namespace VoyagerController.UI
{
    public class EffectSettingsMenu : Menu
    {
        [SerializeField] private IntField _fpsField = null;
        [SerializeField] private IntField _liftField = null;
        [SerializeField] private IntField _contrastField = null;
        [SerializeField] private IntField _saturationField = null;
        [SerializeField] private IntField _blurField = null;
        [SerializeField] private EffectDisplaySettings _displaySettings = null;

        private Effect _effect;

        public void SetEffect(Effect effect)
        {
            _effect = effect;

            UnsubscribeFields();

            if (_effect is VideoEffect video)
            {
                _fpsField.SetValue((int)video.Video.Fps);
                _fpsField.gameObject.SetActive(true);
            }
            else
            {
                _fpsField.gameObject.SetActive(false);
            }

            _blurField.gameObject.SetActive(!Application.isMobilePlatform);

            _liftField.SetValue(effect.Settings.Lift);
            _contrastField.SetValue(effect.Settings.Contrast);
            _saturationField.SetValue(effect.Settings.Saturation);
            _blurField.SetValue(effect.Settings.Blur);

            SubscribeFields();
        }
        
        private void SubscribeFields()
        {
            _fpsField.OnChanged += FpsChanged;
            _liftField.OnChanged += LiftChanged;
            _contrastField.OnChanged += ContrastChanged;
            _saturationField.OnChanged += SaturationChanged;
            _blurField.OnChanged += BlurChanged;
        }

        private void UnsubscribeFields()
        {
            _fpsField.OnChanged -= FpsChanged;
            _liftField.OnChanged -= LiftChanged;
            _contrastField.OnChanged -= ContrastChanged;
            _saturationField.OnChanged -= SaturationChanged;
            _blurField.OnChanged -= BlurChanged;
        }

        private void FpsChanged(int value)
        {
            if (_effect is VideoEffect video)
            {
                video.Video.Fps = value;
                _displaySettings.GetComponent<VideoEffectDisplay>().SetFps(value);
                foreach (var item in WorkspaceManager.GetItems<VoyagerItem>())
                    item.LampHandle.SetFps(value);
            }
        }

        private void LiftChanged(int value)
        {
            _effect.Settings.Lift = _liftField.Normalized;
            AfterEffectChanged();
        }

        private void ContrastChanged(int value)
        {
            _effect.Settings.Contrast = _contrastField.Normalized;
            AfterEffectChanged();
        }

        private void SaturationChanged(int value)
        {
            _effect.Settings.Saturation = _saturationField.Normalized;
            AfterEffectChanged();
        }

        private void BlurChanged(int value)
        {
            _effect.Settings.Blur = _blurField.Normalized;
            AfterEffectChanged();
        }

        private void AfterEffectChanged()
        {
            EffectManager.InvokeEffectModified(_effect);
            _displaySettings.UpdateSettings(_effect);

            if (_effect is VideoEffect || _effect is ImageEffect)
            {
                foreach (var item in WorkspaceManager.GetItems<VoyagerItem>())
                {
                    var voyager = item.LampHandle;
                    LampEffectsWorker.ApplyEffectToLamp(voyager, _effect);
                }
            }
        }
    }
}
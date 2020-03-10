using UnityEngine;
using VoyagerApp.Effects;
using VoyagerApp.Lamps;
using VoyagerApp.Networking.Voyager;
using VoyagerApp.Utilities;

namespace VoyagerApp.UI.Menus
{
    public class EffectSettingsMenu : Menu
    {
        [SerializeField] VideoMapper _videoMapper = null;
        [SerializeField] StreamMapper _streamMapper = null;

        [SerializeField] IntField _fpsField = null;
        [SerializeField] IntField _liftField = null;
        [SerializeField] IntField _contrastField = null;
        [SerializeField] IntField _saturationField = null;
        [SerializeField] IntField _blurField = null;

        Effect _effect;
        bool _fieldsInitialized;

        public void SetEffect(Effect effect)
        {
            _effect = effect;

            if (_fieldsInitialized)
                UnsubscribeFields();

            if (_effect is Video videoEffect)
            {
                _fpsField.SetValue(videoEffect.fps);
                _fpsField.gameObject.SetActive(true);
            }
            else
            {
                _fpsField.gameObject.SetActive(false);
            }

            _blurField.gameObject.SetActive(!Application.isMobilePlatform);

            _liftField.SetValue(effect.lift);
            _contrastField.SetValue(effect.contrast);
            _saturationField.SetValue(effect.saturation);
            _blurField.SetValue(effect.blur);

            SubscribeFields();
            _fieldsInitialized = true;

            if (_effect is Video)
                _videoMapper.UpdateEffectSettings();
        }

        void SubscribeFields()
        {
            _fpsField.onChanged += FpsChanged;
            _liftField.onChanged += LiftChanged;
            _contrastField.onChanged += ContrastChanged;
            _saturationField.onChanged += SaturationChanged;
            _blurField.onChanged += BlurChanged;
        }

        void UnsubscribeFields()
        {
            _fpsField.onChanged -= FpsChanged;
            _liftField.onChanged -= LiftChanged;
            _contrastField.onChanged -= ContrastChanged;
            _saturationField.onChanged -= SaturationChanged;
            _blurField.onChanged -= BlurChanged;
        }

        void LiftChanged(int value)
        {
            _effect.lift = _liftField.normalized;
            AfterEffectChanged();
        }

        void ContrastChanged(int value)
        {
            _effect.contrast = _contrastField.normalized;
            AfterEffectChanged();
        }

        void SaturationChanged(int value)
        {
            _effect.saturation = _saturationField.normalized;
            AfterEffectChanged();
        }

        void BlurChanged(int value)
        {
            _effect.blur = _blurField.normalized;
            AfterEffectChanged();
        }

        void AfterEffectChanged()
        {
            EffectManager.instance.InvokeEffectChange(_effect);
            
            if (_effect is Video)
            {
                _videoMapper.UpdateEffectSettings();
                foreach (var lamp in WorkspaceUtils.VoyagerLamps)
                {
                    lamp.effect = null;
                    lamp.SetEffect(_effect);
                }
            }
            else
            {
                _streamMapper.UpdateEffectSettings();
            }
        }

        void FpsChanged(int value)
        {
            ((Video)_effect).fps = value;

            foreach (var lamp in WorkspaceUtils.Lamps)
                SendFpsPacket(lamp, value);

            _videoMapper.SetFps(value);
        }

        void SendFpsPacket(Lamp lamp, int fps)
        {
            var packet = new SetFpsPacket(fps);
            NetUtils.VoyagerClient.KeepSendingPacket(
                lamp,
                "set_fps",
                packet,
                VoyagerClient.PORT_SETTINGS,
                TimeUtils.Epoch);
        }
    }
}
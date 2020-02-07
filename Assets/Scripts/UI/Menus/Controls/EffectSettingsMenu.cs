using UnityEngine;
using VoyagerApp.Effects;
using VoyagerApp.Lamps;
using VoyagerApp.Networking.Voyager;
using VoyagerApp.Utilities;

namespace VoyagerApp.UI.Menus
{
    public class EffectSettingsMenu : Menu
    {
        [SerializeField] VideoMapper _videoMapper;

        [SerializeField] IntField _fpsField;
        [SerializeField] IntField _liftField;
        [SerializeField] IntField _contrastField;
        [SerializeField] IntField _saturationField;
        [SerializeField] IntField _blurField;

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

            _liftField.SetValue(effect.lift);
            _contrastField.SetValue(effect.contrast);
            _saturationField.SetValue(effect.saturation);

            SubscribeFields();
            _fieldsInitialized = true;
            _videoMapper.UpdateEffectSettings();
        }

        void SubscribeFields()
        {
            _fpsField.onChanged += FpsChanged;
            _liftField.onChanged += LiftChanged;
            _contrastField.onChanged += ContrastChanged;
            _saturationField.onChanged += SaturationChanged;
        }

        void UnsubscribeFields()
        {
            _fpsField.onChanged -= FpsChanged;
            _liftField.onChanged -= LiftChanged;
            _contrastField.onChanged -= ContrastChanged;
            _saturationField.onChanged -= SaturationChanged;
        }

        void LiftChanged(int value)
        {
            _effect.lift = _liftField.normalized;
            _videoMapper.UpdateEffectSettings();
        }

        void ContrastChanged(int value)
        {
            _effect.contrast = _contrastField.normalized;
            _videoMapper.UpdateEffectSettings();
        }

        void SaturationChanged(int value)
        {
            _effect.saturation = _saturationField.normalized;
            _videoMapper.UpdateEffectSettings();
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
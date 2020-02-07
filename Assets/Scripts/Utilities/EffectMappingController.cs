using System.Collections.Generic;
using UnityEngine;
using VoyagerApp.Effects;
using VoyagerApp.Lamps;
using VoyagerApp.UI.Menus;
using VoyagerApp.Videos;
using VoyagerApp.Workspace;
using VoyagerApp.Utilities;
using UnityEngine.UI;

namespace VoyagerApp.UI
{
    public class EffectMappingController : MonoBehaviour
    {
        [SerializeField] EffectMappingMenu _mappingMenu = null;
        [SerializeField] EffectSettingsMenu _settingsMenu = null;
        [SerializeField] VideoMapper _videoMapper = null;
        [SerializeField] StreamMapper _streamMapper = null;

        [SerializeField] Button[] _playPauseStopBtns;

        void Start() => LoadCorrectEffect();

        Lamp _keepLamp;
        Effect _prevEffect;

        void LoadCorrectEffect()
        {
            EffectMappingSettings settings = EffectMappingSettings.Load();

            var lamps = LampsFromSerials(settings.lamps);
            var effect = EffectManager.GetEffectWithId(settings.effect);

            SetEffect(effect);
            PositionLampsBasedOnEffect(effect, lamps);

            _keepLamp = lamps[0];
            LampManager.instance.onLampEffectChanged += LampEffectChanged;
        }

        void OnDestroy()
        {
            LampManager.instance.onLampEffectChanged -= LampEffectChanged;
        }

        void LampEffectChanged(Lamp lamp)
        {
            if (lamp == _keepLamp && lamp.effect != _prevEffect)
            {
                var lamps = WorkspaceUtils.Lamps;
                WorkspaceManager.instance.Clear();
                SetEffect(lamp.effect);
                PositionLampsBasedOnEffect(lamp.effect, lamps);
            }
        }

        public void SetEffect(Effect effect)
        {
            if (effect is Video video)
            {
                _videoMapper.SetVideo(video);
                foreach (var button in _playPauseStopBtns)
                    if (button != null) button.interactable = true;
            }
            else
            {
                _streamMapper.SetEffect(effect);
                foreach (var button in _playPauseStopBtns)
                    if (button != null) button.interactable = false;
            }

            _mappingMenu.SetEffect(effect);
            _settingsMenu.SetEffect(effect);
            _prevEffect = effect;
        }

        void PositionLampsBasedOnEffect(Effect effect, List<Lamp> lamps)
        {
            if (effect is Video)
            {
                PositionLamps(lamps, _videoMapper.transform);
                _videoMapper.gameObject.SetActive(true);
                _streamMapper.gameObject.SetActive(false);
            }
            if (effect is SyphonStream || effect is SpoutStream)
            {
                PositionLamps(lamps, _streamMapper.transform);
                _videoMapper.gameObject.SetActive(false);
                _streamMapper.gameObject.SetActive(true);
            }
        }

        public void PositionLamps(List<Lamp> lamps, Transform trans)
        {
            foreach (var lamp in lamps)
            {
                var position = GetLampVideoPosition(lamp, trans);
                var scale = GetLampVideoScale(lamp, trans);
                var rotation = GetLampVideoRotation(lamp, trans);
                var view = lamp.AddToWorkspace(position, scale, rotation);
                WorkspaceSelection.instance.SelectItem(view);
            }
        }

        List<Lamp> LampsFromSerials(string[] serials)
        {
            List<Lamp> lamps = new List<Lamp>();

            foreach (var serial in serials)
            {
                Lamp lamp = LampManager.instance.GetLampWithSerial(serial);
                if (lamp != null) lamps.Add(lamp);
            }

            return lamps;
        }

        Vector2 GetLampVideoPosition(Lamp lamp, Transform trans)
        {
            float x = (lamp.mapping.p1.x + lamp.mapping.p2.x) / 2.0f - 0.5f;
            float y = (lamp.mapping.p1.y + lamp.mapping.p2.y) / 2.0f - 0.5f;
            return trans.TransformPoint(x, y, 0);
        }

        float GetLampVideoScale(Lamp lamp, Transform trans)
        {
            Vector2 start = lamp.mapping.p1;
            Vector2 end = lamp.mapping.p2;

            Vector2 wStart = trans.TransformPoint(start);
            Vector2 wEnd = trans.TransformPoint(end);

            float distance = Vector2.Distance(wStart, wEnd);
            return distance / (lamp.pixels * 0.15f);
        }

        float GetLampVideoRotation(Lamp lamp, Transform trans)
        {
            Vector2 start = lamp.mapping.p1;
            Vector2 end = lamp.mapping.p2;

            Vector2 wStart = trans.TransformPoint(start);
            Vector2 wEnd = trans.TransformPoint(end);

            return VectorUtils.AngleFromTo(wStart, wEnd);
        }
    }
}

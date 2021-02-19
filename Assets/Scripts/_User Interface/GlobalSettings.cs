using DigitalSputnik;
using UnityEngine;

namespace VoyagerController.UI
{
    public class GlobalSettings : MonoBehaviour
    {
        [SerializeField] private GameObject _playBtn = null;
        [SerializeField] private GameObject _pauseBtn = null;
        [SerializeField] private IntField _dimmer = null;

        private void Start()
        {
            _dimmer.SetValue(ApplicationState.GlobalDimmer.Value);
            _dimmer.OnChanged += DimmerFieldChanged;

            PlaymodeChanged(ApplicationState.Playmode.Value);
            ApplicationState.Playmode.OnChanged += PlaymodeChanged;
        }

        private void OnDestroy()
        {
            _dimmer.OnChanged -= DimmerFieldChanged;
            ApplicationState.Playmode.OnChanged -= PlaymodeChanged;
        }

        public void Play()
        {
            LampEffectsWorker.ModifyVideoStartTime();
            ApplicationState.Playmode.Value = GlobalPlaymode.Play;
        }

        public void Pause()
        {
            ApplicationState.PlaymodePausedSince.Value = TimeUtils.Epoch;
            ApplicationState.Playmode.Value = GlobalPlaymode.Pause;
        }

        public void Stop()
        {
            ApplicationState.PlaymodePausedSince.Value = -1.0;
            ApplicationState.Playmode.Value = GlobalPlaymode.Stop;
        }

        private void PlaymodeChanged(GlobalPlaymode mode)
        {
            switch (mode)
            {
                case GlobalPlaymode.Play:
                    _playBtn.SetActive(false);
                    _pauseBtn.SetActive(true);
                    break;
                case GlobalPlaymode.Pause:
                case GlobalPlaymode.Stop:
                    _playBtn.SetActive(true);
                    _pauseBtn.SetActive(false);
                    break;
            }
        }

        private void DimmerFieldChanged(int value) => ApplicationState.GlobalDimmer.Value = _dimmer.Normalized;
    }
}
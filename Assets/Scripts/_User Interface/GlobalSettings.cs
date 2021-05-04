using System.Linq;
using DigitalSputnik;
using UnityEngine;
using UnityEngine.UI;
using VoyagerController.Bluetooth;
using VoyagerController.Workspace;

namespace VoyagerController.UI
{
    public class GlobalSettings : MonoBehaviour
    {
        [SerializeField] private Button _playBtn = null;
        [SerializeField] private Button _pauseBtn = null;
        [SerializeField] private Button _stopBtn = null;
        [SerializeField] private IntField _dimmer = null;

        private void Start()
        {
            _dimmer.SetValue(ApplicationState.GlobalDimmer.Value);
            _dimmer.OnChanged += DimmerFieldChanged;

            PlaymodeChanged(ApplicationState.Playmode.Value);
            ApplicationState.Playmode.OnChanged += PlaymodeChanged;
            WorkspaceSelection.SelectionChanged += SelectionChanged;
        }

        private void OnDestroy()
        {
            _dimmer.OnChanged -= DimmerFieldChanged;
            ApplicationState.Playmode.OnChanged -= PlaymodeChanged;
            WorkspaceSelection.SelectionChanged -= SelectionChanged;
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
                    _playBtn.gameObject.SetActive(false);
                    _pauseBtn.gameObject.SetActive(true);
                    break;
                case GlobalPlaymode.Pause:
                case GlobalPlaymode.Stop:
                    _playBtn.gameObject.SetActive(true);
                    _pauseBtn.gameObject.SetActive(false);
                    break;
            }
        }

        private void SelectionChanged()
        {
            var ble = WorkspaceSelection
                .GetSelected<VoyagerItem>()
                .Any(v => v.LampHandle.Endpoint is BluetoothEndPoint);

            _playBtn.interactable = !ble;
            _pauseBtn.interactable = !ble;
            _stopBtn.interactable = !ble;
            _dimmer.Interactable = !ble;
        }

        private void DimmerFieldChanged(int value) => ApplicationState.GlobalDimmer.Value = _dimmer.Normalized;
    }
}
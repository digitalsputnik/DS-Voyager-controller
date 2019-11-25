using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using VoyagerApp.Lamps;
using VoyagerApp.Networking.Voyager;
using VoyagerApp.UI.Menus;
using VoyagerApp.Utilities;
using VoyagerApp.Videos;

namespace VoyagerApp.UI
{
    public class PlayPauseStop : MonoBehaviour
    {
        [SerializeField] GameObject playButton = null;
        [SerializeField] GameObject pauseButton = null;
        [SerializeField] IntField globalDimmerField = null;

        void Start()
        {
            globalDimmerField.SetValue(ApplicationState.GlobalDimmer.value);
            GlobalPlaymodeChanged(ApplicationState.Playmode.value);

            globalDimmerField.onChanged += GlobalDimmerFieldChanged;
            ApplicationState.GlobalDimmer.onChanged += GlobalDimmerValueChanged;
            ApplicationState.Playmode.onChanged += GlobalPlaymodeChanged;
        }

        void OnDestroy()
        {
            globalDimmerField.onChanged -= GlobalDimmerFieldChanged;
            ApplicationState.GlobalDimmer.onChanged -= GlobalDimmerValueChanged;
            ApplicationState.Playmode.onChanged -= GlobalPlaymodeChanged;
        }

        void GlobalDimmerFieldChanged(int value)
        {
            ApplicationState.GlobalDimmer.value = (float)math.round(value / 100.0);
        }

        void GlobalDimmerValueChanged(float value)
        {
            var packet = new SetGlobalIntensityPacket(value);
            foreach (var lamp in LampManager.instance.Lamps.Where(l => l.connected))
                NetUtils.VoyagerClient.SendPacket(lamp, packet, VoyagerClient.PORT_SETTINGS);
        }

        void GlobalPlaymodeChanged(GlobalPlaymode value)
        {
            switch (value)
            {
                case GlobalPlaymode.Play:
                    playButton.SetActive(false);
                    pauseButton.SetActive(true);
                    break;
                case GlobalPlaymode.Pause:
                case GlobalPlaymode.Stop:
                    playButton.SetActive(true);
                    pauseButton.SetActive(false);
                    break;
            }
        }

        void ModifyVideoStartTime()
        {
            if (ApplicationState.PlaymodePausedSince.value > 0.0)
            {
                var pauseTime = TimeUtils.Epoch - ApplicationState.PlaymodePausedSince.value;
                foreach (var video in VideoManager.instance.Videos)
                    video.lastStartTime += pauseTime;
            }
            else
            {
                foreach (var video in VideoManager.instance.Videos)
                    video.lastStartTime = TimeUtils.Epoch;
            }
        }

        public void Play()
        {
            ApplicationState.Playmode.value = GlobalPlaymode.Play;
            ModifyVideoStartTime();
        }
        public void Pause()
        {
            ApplicationState.PlaymodePausedSince.value = TimeUtils.Epoch;
            ApplicationState.Playmode.value = GlobalPlaymode.Pause;
        }
        public void Stop()
        {
            ApplicationState.PlaymodePausedSince.value = -1.0;
            ApplicationState.Playmode.value = GlobalPlaymode.Stop;
        }
    }
}
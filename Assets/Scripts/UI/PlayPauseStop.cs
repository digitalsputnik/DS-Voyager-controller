using System.Linq;
using UnityEngine;
using VoyagerApp.Lamps;
using VoyagerApp.Networking.Voyager;
using VoyagerApp.UI.Menus;
using VoyagerApp.Utilities;

namespace VoyagerApp.UI
{
    public class PlayPauseStop : MonoBehaviour
    {
        public static bool playing;

        public static event PlaymodePlayHandler onPlay;
        public static event PlaymodeHandler onPause;
        public static event PlaymodeHandler onStop;

        [SerializeField] IntField globalDimmerField = null;

        static bool stop;
        static bool pause;
        static double latestPauseStartTime;

        void Start()
        {
            globalDimmerField.onChanged += GlobalDimmerValueChanged;
        }

        void OnDestroy()
        {
            globalDimmerField.onChanged -= GlobalDimmerValueChanged;
        }

        void GlobalDimmerValueChanged(int value)
        {
            var packet = new SetGlobalIntensityPacket(globalDimmerField.normalized);
            foreach (var lamp in LampManager.instance.Lamps.Where(l => l.connected))
                NetUtils.VoyagerClient.SendPacket(lamp, packet, VoyagerClient.PORT_SETTINGS);
        }

        public void Play()
        {
            double pauseTime = -1.0;
            if (pause)
                pauseTime = TimeUtils.Epoch - latestPauseStartTime;

            onPlay?.Invoke(pauseTime, stop);

            stop = false;
            pause = false;
            playing = true;
        }
        public void Pause()
        {
            onPause?.Invoke();
            latestPauseStartTime = TimeUtils.Epoch;
            pause = true;
            playing = false;
        }
        public void Stop()
        {
            onStop?.Invoke();
            stop = true;
            playing = false;
        }
    }

    public delegate void PlaymodePlayHandler(double pauseTime, bool fromStop);
    public delegate void PlaymodeHandler();
}
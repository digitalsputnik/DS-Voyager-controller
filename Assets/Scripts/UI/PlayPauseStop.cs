using UnityEngine;
using VoyagerApp.Utilities;

namespace VoyagerApp.UI
{
    public class PlayPauseStop : MonoBehaviour
    {
        public static event PlaymodePlayHandler onPlay;
        public static event PlaymodeHandler onPause;
        public static event PlaymodeHandler onStop;

        static bool stop;
        static bool pause;
        static double latestPauseStartTime;

        public void Play()
        {
            double pauseTime = -1.0;
            if (pause)
                pauseTime = TimeUtils.Epoch - latestPauseStartTime;

            onPlay?.Invoke(pauseTime, stop);

            stop = false;
            pause = false;
        }
        public void Pause()
        {
            onPause?.Invoke();
            latestPauseStartTime = TimeUtils.Epoch;
            pause = true;
        }
        public void Stop()
        {
            onStop?.Invoke();
            stop = true;
        }
    }

    public delegate void PlaymodePlayHandler(double pauseTime, bool fromStop);
    public delegate void PlaymodeHandler();
}
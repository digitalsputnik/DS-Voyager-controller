using UnityEngine;

namespace VoyagerApp.UI
{
    public class PlayPauseStop : MonoBehaviour
    {
        public static event PlaymodeHandler onPlay;
        public static event PlaymodeHandler onPause;
        public static event PlaymodeHandler onStop;

        public void Play() => onPlay?.Invoke();
        public void Pause() => onPause?.Invoke();
        public void Stop() => onStop?.Invoke();
    }

    public delegate void PlaymodeHandler();
}
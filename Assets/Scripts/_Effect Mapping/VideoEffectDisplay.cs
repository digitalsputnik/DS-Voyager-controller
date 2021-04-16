using System.Linq;
using DigitalSputnik.Videos;
using DigitalSputnik.Voyager;
using UnityEngine.Video;
using VoyagerController.Effects;
using VoyagerController.Workspace;

namespace VoyagerController.Mapping
{
    public class VideoEffectDisplay : EffectDisplay
    {
        private const float CORRECT_FRAME_RATE = 15.0f;
        
        private VideoPlayer _player;
        private Video _video;
        private VoyagerLamp _lamp;

        public override void Setup(Effect effect)
        {
            if (effect is VideoEffect videoEffect)
            {
                _player = GetComponent<VideoPlayer>();
                _video = videoEffect.Video;
                _lamp = WorkspaceManager.GetItems<VoyagerItem>().First().LampHandle;
                
                _player.enabled = true;
                _player.url = _video.Path;
                _player.prepareCompleted += PlayerPrepared;
                _player.loopPointReached += LoopPointReached;
                _player.Prepare();
                
                ApplicationState.Playmode.OnChanged += PlaymodeChanged;
            }
        }

        private void PlayerPrepared(VideoPlayer source)
        {
            PlaymodeChanged(ApplicationState.Playmode.Value);
            InvokeRepeating(nameof(CorrectFrame), 0.0f, CORRECT_FRAME_RATE);
        }
        
        private void LoopPointReached(VideoPlayer source)
        {
            CorrectFrame();
        }

        private void CorrectFrame()
        {
            var index = LampEffectsWorker.GetCurrentFrameOfVideo(_lamp, _video) + 2;
            while (index >= (long) _video.FrameCount) index -= (long) _video.FrameCount;
            _player.frame = index;
        }

        public void SetFps(float fps)
        {
            _player.playbackSpeed = 1.0f / _player.frameRate * fps;
            CorrectFrame();
        }
        
        private void PlaymodeChanged(GlobalPlaymode value)
        {
            CorrectFrame();
            switch (value)
            {
                case GlobalPlaymode.Play:
                    _player.Play();
                    break;
                case GlobalPlaymode.Pause:
                case GlobalPlaymode.Stop:
                    _player.Pause();
                    break;
            }
        }

        public override void Clean()
        {
            StopAllCoroutines();
            _player.prepareCompleted -= PlayerPrepared;
            _player.loopPointReached -= LoopPointReached;
            _player.enabled = false;
            
            ApplicationState.Playmode.OnChanged -= PlaymodeChanged;
        }
    }
}
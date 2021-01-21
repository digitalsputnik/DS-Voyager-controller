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
                _player.Prepare();
            }
        }

        private void PlayerPrepared(VideoPlayer source)
        {
            _player.Play();
            InvokeRepeating(nameof(CorrectFrame), 0.0f, CORRECT_FRAME_RATE);
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

        public override void Clean()
        {
            StopAllCoroutines();
            _player.prepareCompleted -= PlayerPrepared;
            _player.enabled = false;
        }
    }
}
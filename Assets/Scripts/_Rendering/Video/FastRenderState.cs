using System.Collections.Generic;
using System.Linq;
using DigitalSputnik;
using DigitalSputnik.Voyager;
using UnityEngine;
using VoyagerController.Effects;

namespace VoyagerController.Rendering
{
    internal class FastRenderState : VideoRenderState
    {
        private const int START_BEFORE = 3;
        private const int RETRY_FAST_RENDER = 3;
        
        private readonly RenderQueue _queue;
        private readonly VideoEffect _effect;
        private readonly VoyagerLamp[] _lamps;

        private FastRendererState _state = FastRendererState.Prepare;
        
        private ulong _prevIndex = ulong.MaxValue;
        private Texture2D _texture;

        private List<MissingFrame> _missingFrames;
        private int _currentFrame = -1;
        private MissingFrame CurrentFrame => _missingFrames[_currentFrame];
        private int _framesRendererd;

        public FastRenderState(RenderQueue queue, VideoEffect current)
        {
            _queue = queue;
            _effect = current;
            _lamps = GetLampsWithEffect(_effect).ToArray();
            
            StartFastRenderLoop();
        }

        internal override VideoRenderState Update()
        {
            if (Finished) return new InterpolationState(_queue, _effect);

            switch (_state)
            {
                case FastRendererState.Prepare:
                    StartFastRenderLoop();
                    SeekToNextMissingFrame();
                    break;
                case FastRendererState.Seeked:
                    Seeked();
                    break;
                case FastRendererState.Render:
                    Render();
                    break;
            }

            return this;
        }

        private bool Finished => _missingFrames.All(f => f.Requested >= RETRY_FAST_RENDER || f.Received);

        private void StartFastRenderLoop()
        {
            _missingFrames = GenerateMissingFramesList();
        }

        private void SeekToNextMissingFrame()
        {
            IncreaseFrameToNextMissing();
            SeekToFrame(CurrentFrame.Frame);
        }

        private void IncreaseFrameToNextMissing()
        {
            do { IncreaseFrameIndex(); } while (CurrentFrame.Received);
        }

        private void IncreaseFrameIndex()
        {
            _currentFrame++;
            if (_currentFrame >= _missingFrames.Count)
                _currentFrame = 0;
        }

        private void Render()
        {
            var player = VideoEffectRenderer.VideoPlayer;
            var index = (ulong) player.frame;

            if (index == _prevIndex) return;
            
            _prevIndex = index;

            if (index == CurrentFrame.Frame)
            {
                if (index >= _effect.Video.FrameCount) return;

                if (_texture != null) Object.Destroy(_texture);

                _texture = VideoEffectRenderer.GetFrameTexture2D(_effect);

                foreach (var voyager in _lamps)
                {
                    var colors = RenderLampColors(voyager, _texture);
                    SendColorsToLamp(voyager, colors, (long)index);
                }

                CurrentFrame.Received = true;
            }

            _framesRendererd++;
            
            if (_framesRendererd >= START_BEFORE)
            {
                CurrentFrame.Requested++;
                SeekToNextMissingFrame();
            }
        }

        private void Seeked()
        {
            _framesRendererd = 0;
            _state = FastRendererState.Render;
        }
        
        private static Color32[] RenderLampColors(VoyagerLamp lamp, Texture2D frame)
        {
            var coords = TextureExtensions.MapLampToVideoCoords(lamp, frame);
            return TextureExtensions.CoordsToColors(coords.ToArray(), frame);
        }

        private static void SendColorsToLamp(VoyagerLamp voyager, Color32[] colors, long index)
        {
            var data = colors.ToRgbArray();
            LampEffectsWorker.ApplyVideoFrameToVoyager(voyager, index, data);
        }

        private List<MissingFrame> GenerateMissingFramesList()
        {
            var missing = new List<MissingFrame>();

            foreach (var voyager in GetLampsWithEffect(_effect))
            {
                var buffer = Metadata.Get<LampData>(voyager.Serial).FrameBuffer;
                var pixels = voyager.PixelCount;
                
                for (ulong i = 0; i < _effect.Video.FrameCount; i++)
                {
                    if (buffer[i] != null && buffer[i].Length == pixels) continue;
                    if (missing.All(f => f.Frame != i))
                        missing.Add(new MissingFrame(i));
                }
            }
            
            return missing;
        }

        private void SeekToFrame(ulong frame)
        {
            _state = FastRendererState.Seeking;
            VideoEffectRenderer.VideoPlayer.frame = GetRoundedFrame(frame);
            VideoEffectRenderer.VideoPlayer.seekCompleted += source =>
            {
                _state = FastRendererState.Seeked;
            };
        }

        private long GetRoundedFrame(ulong frame)
        {
            var count = (long) _effect.Video.FrameCount;
            var wanted = (long) frame - START_BEFORE;
            if (wanted < 0)
                wanted += count;
            if (wanted >= count)
                wanted -= count;
            return wanted;
        }

        private static IEnumerable<VoyagerLamp> GetLampsWithEffect(Effect effect)
        {
            return LampManager.Instance
                .GetLampsOfType<VoyagerLamp>()
                .Where(l => Metadata.Get<LampData>(l.Serial).Effect == effect);
        }

        private enum FastRendererState
        {
            Prepare,
            Seeking,
            Seeked,
            Render
        }

        private class MissingFrame
        {
            public ulong Frame { get; }
            public int Requested { get; set; }
            public bool Received { get; set; }

            public MissingFrame(ulong frame)
            {
                Frame = frame;
                Requested = 0;
                Received = false;
            }
        }
    }
}
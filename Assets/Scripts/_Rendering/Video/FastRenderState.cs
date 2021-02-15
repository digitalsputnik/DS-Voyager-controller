using System;
using System.Collections.Generic;
using System.Linq;
using DigitalSputnik;
using DigitalSputnik.Voyager;
using UnityEngine;
using VoyagerController.Effects;
using Object = UnityEngine.Object;

namespace VoyagerController.Rendering
{
    internal class FastRenderState : VideoRenderState
    {
        private const int START_BEFORE = 3;
        private const int RETRY_FAST_RENDER = 3;
        
        private readonly RenderQueue _queue;
        private readonly VideoEffect _effect;
        private readonly VoyagerLamp[] _lamps;
        private List<ulong> _missingFrames;

        private FastRendererState _state = FastRendererState.Prepare;
        private bool _renderLoopFinished;
        private int _loopTime = 0;
        private List<ulong> _framesInFocus = new List<ulong>();
        
        private ulong _prevIndex = ulong.MaxValue;
        private Texture2D _texture;
        private ulong _framesToRender;
        private ulong _framesRendered;
        
        public FastRenderState(RenderQueue queue, VideoEffect current)
        {
            _queue = queue;
            _effect = current;
            _lamps = GetLampsWithEffect(_effect).ToArray();
            
            StartFastRenderLoop();
        }

        internal override VideoRenderState Update()
        {
            if (_renderLoopFinished && _loopTime >= RETRY_FAST_RENDER)
            {
                return new DisposeState(); // Start interpolation state here.
            }

            switch (_state)
            {
                case FastRendererState.Prepare:
                    StartFastRenderLoop();
                    _state = FastRendererState.Done;
                    break;
                case FastRendererState.Render:
                    Render();
                    break;
                case FastRendererState.Done:
                    Done();
                    break;
                case FastRendererState.Seeked:
                    Seeked();
                    break;
            }

            return this;
        }

        private void StartFastRenderLoop()
        {
            _missingFrames = GenerateMissingFramesList();
            _renderLoopFinished = false;
        }

        private void Done()
        {
            if (LoopFinished())
                FinishLoop();
            else
            {
                _framesInFocus = GetFramesToFocus();
                SeekToFrame(_framesInFocus.First());
            }
        }

        private void Render()
        {
            var player = VideoEffectRenderer.VideoPlayer;
            var index = (ulong) player.frame;

            if (index == _prevIndex) return;
            
            _prevIndex = index;
            
            if (_missingFrames.Contains(index))
            {
                if (index >= _effect.Video.FrameCount) return;

                if (_texture != null) Object.Destroy(_texture);

                _texture = VideoEffectRenderer.GetFrameTexture2D(_effect);

                foreach (var voyager in _lamps)
                {
                    var colors = RenderLampColors(voyager, _texture);
                    SendColorsToLamp(voyager, colors, (long)index);
                }

                _missingFrames.Remove(index);
            }

            _framesRendered++;

            if (_framesRendered >= _framesToRender)
                _state = FastRendererState.Done;
        }

        private void Seeked()
        {
            _framesToRender = _framesInFocus.Last() - _framesInFocus.First() + START_BEFORE * 2;
            _framesRendered = 0;
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

        private List<ulong> GenerateMissingFramesList()
        {
            var missing = new List<ulong>();

            foreach (var voyager in GetLampsWithEffect(_effect))
            {
                var buffer = Metadata.Get(voyager.Serial).FrameBuffer;
                var pixels = voyager.PixelCount;
                
                for (ulong i = 0; i < _effect.Video.FrameCount; i++)
                {
                    if (buffer[i] != null && buffer[i].Length == pixels) continue;
                    if (!missing.Contains(i)) missing.Add(i);
                }
            }
            
            return missing;
        }

        private bool LoopFinished()
        {
            return _missingFrames.Count == 0;
        }
        
        private void FinishLoop()
        {
            _loopTime++;
            _renderLoopFinished = true;
            _framesInFocus.Clear();
            _state = FastRendererState.Prepare;
        }

        private List<ulong> GetFramesToFocus()
        {
            var first = _missingFrames.FirstOrDefault();
            var index = _missingFrames.IndexOf(first);
            var frames = new List<ulong> { first };

            while (true)
            {
                index++;
             
                if (index >= _missingFrames.Count) break;
                if (_missingFrames[index] - first > START_BEFORE) break;
                
                frames.Add(_missingFrames[index]);
                first = _missingFrames[index];
            }

            return frames;
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
                .Where(l => Metadata.Get(l.Serial).Effect == effect);
        }

        private enum FastRendererState
        {
            Prepare,
            Seeking,
            Seeked,
            Render,
            Done
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using DigitalSputnik.Voyager;
using UnityEngine;
using VoyagerController.Effects;
using Object = UnityEngine.Object;

namespace VoyagerController.Rendering
{
    internal class RenderState : VideoRenderState, IDisposable
    {
        private const long FRAMES_TO_ADD_AT_START = 5;
        
        private RenderQueue _queue;
        private VideoEffect _effect;
        private List<VoyagerLamp> _lamps;
        private long _prevVideoIndex;

        private bool _playerPrepared = false;
        private Texture2D _texture;
        
        public RenderState(RenderQueue queue)
        {
            _queue = queue;
            DequeueNextEffect();
        }
        
        internal override VideoRenderState Update()
        {
            if (CurrentEffectRendered())
            {
                if (!QueueEmpty())
                    DequeueNextEffect();
                else
                    return new DisposeState();
            }

            RenderFrames();
            return this;
        }

        private void RenderFrames()
        {
            var player = VideoEffectRenderer.VideoPlayer;
            
            if (!_playerPrepared || !player.isPlaying || player.frame == _prevVideoIndex)
                return;
            
            var index = player.frame;

            if (index >= (long) _effect.Video.FrameCount)
                return;

            if (_texture != null) Object.Destroy(_texture);

            _texture = VideoEffectRenderer.GetFrameTexture2D(_effect);

            foreach (var voyager in _lamps)
            {
                var colors = RenderLampColors(voyager, _texture);
                SendColorsToLamp(voyager, colors, index);
            }

            _prevVideoIndex = index;
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

        private void DequeueNextEffect()
        {
            var pair = _queue.Dequeue();
            _effect = pair.Key;
            _lamps = pair.Value;
            _prevVideoIndex = -1;
            _playerPrepared = false;
            
            VideoEffectRenderer.PrepareVideoPlayer(_effect.Video, () =>
            {
                VideoEffectRenderer.VideoPlayer.Play();
                VideoEffectRenderer.VideoPlayer.frame =
                    LampEffectsWorker.GetCurrentFrameOfVideo(_lamps[0], _effect.Video) + FRAMES_TO_ADD_AT_START;
                VideoEffectRenderer.VideoPlayer.seekCompleted += source =>
                {
                    _playerPrepared = true;
                };
            });
        }

        private bool QueueEmpty() => _queue.Count == 0;

        private bool CurrentEffectRendered()
        {
            return _lamps.All(l => Metadata.Get(l.Serial).Effect is VideoEffect && Metadata.Get(l.Serial).Rendered);
        }
        
        public void Dispose()
        {
            if (_texture != null) Object.Destroy(_texture);
        }
    }
}
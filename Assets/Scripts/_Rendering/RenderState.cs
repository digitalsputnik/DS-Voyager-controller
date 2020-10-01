using System.Collections.Generic;
using System.Linq;
using DigitalSputnik.Colors;
using DigitalSputnik.Voyager;
using UnityEngine;
using VoyagerController.Effects;

namespace VoyagerController.Rendering
{
    internal class RenderState : VideoRenderState
    {
        private RenderQueue _queue;
        private VideoEffect _effect;
        private List<VoyagerLamp> _lamps;
        private long _prevVideoIndex;

        private bool _playerPrepared = false;
        
        public RenderState(RenderQueue queue)
        {
            _queue = queue;
            DequeueNextEffect();
        }
        
        internal override VideoRenderState Update()
        {
            if (CurrentEffectRendered() && !QueueEmpty())
                DequeueNextEffect();
            else
                return new DisposeState();

            RenderFrames();
            return this;
        }

        private void RenderFrames()
        {
            var player = VideoEffectRenderer.VideoPlayer;
            
            if (!_playerPrepared || !player.isPlaying || player.frame == _prevVideoIndex)
                return;

            var index = player.frame;
            var frame = VideoEffectRenderer.RenderTexture.ToTexture2D();

            foreach (var voyager in _lamps)
            {
                var colors = RenderLampColors(voyager, frame);
                SendColorsToLamp(voyager, colors, index);
            }

            _prevVideoIndex = index;
            
            Object.Destroy(frame);
        }

        private static Color32[] RenderLampColors(VoyagerLamp lamp, Texture2D frame)
        {
            /*
            var coords = VectorUtils.MapLampToVideoCoords(lamp, frame);
            var colors = TextureUtils.CoordsToColors(coords, frame);
            lamp.PushFrame(colors, index);
            */

            return null;
        }

        private static void SendColorsToLamp(VoyagerLamp voyager, Color32[] colors, long index)
        {
            ApplicationManager.Lamps.GetMetadata(voyager.Serial).FrameBuffer[index] = colors.ToRgbArray();
        }

        private void DequeueNextEffect()
        {
            var pair = _queue.Dequeue();
            _effect = pair.Key;
            _lamps = pair.Value;
            _prevVideoIndex = -1;
            
            VideoEffectRenderer.PrepareVideoPlayer(_effect.Video, () =>
            {
                _playerPrepared = true;
                VideoEffectRenderer.VideoPlayer.Play();
            });
        }

        private bool QueueEmpty() => _queue.Count == 0;

        private bool CurrentEffectRendered()
        {
            return _lamps.All(l => ApplicationManager.Lamps.GetMetadata(l.Serial).Rendered);
        }
    }
}
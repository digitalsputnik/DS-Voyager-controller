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
            Debugger.LogInfo("Entered to rendering");
            
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
            var frame = VideoEffectRenderer.RenderTexture.ToTexture2D();

            foreach (var voyager in _lamps)
            {
                var colors = RenderLampColors(voyager, frame);
                SendColorsToLamp(voyager, colors, index);
                Debugger.LogInfo($"Rendered frame {index}");
            }

            _prevVideoIndex = index;
            Object.Destroy(frame);
        }

        private static Color32[] RenderLampColors(VoyagerLamp lamp, Texture2D frame)
        {
            var coords = MapLampToVideoCoords(lamp, frame);
            return CoordsToColors(coords.ToArray(), frame);
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
                _playerPrepared = true;
                VideoEffectRenderer.VideoPlayer.Play();
            });
        }

        private bool QueueEmpty() => _queue.Count == 0;

        private bool CurrentEffectRendered()
        {
            return _lamps.All(l => ApplicationManager.Lamps.GetMetadata(l.Serial).Rendered);
        }
        
        private static Color32[] CoordsToColors(Vector2Int[] coords, Texture2D frame)
        {
            var colors = new Color32[coords.Length];
            for (var i = 0; i < coords.Length; i++)
            {
                if (coords[i].x == -1 && coords[i].y == -1)
                    colors[i] = Color.black;
                else
                    colors[i] = frame.GetPixel(coords[i].x, coords[i].y);
            }
            return colors;
        }

        private static IEnumerable<Vector2Int> MapLampToVideoCoords(VoyagerLamp voyager, Texture2D frame)
        {
            var mapping = ApplicationManager.Lamps.GetMetadata(voyager.Serial).EffectMapping;
            var coords = new Vector2Int[voyager.PixelCount];

            var p1 = new Vector2(mapping.X1, mapping.Y1);
            var p2 = new Vector2(mapping.X2, mapping.Y2);

            var delta = p2 - p1;
            var steps = delta / (coords.Length - 1);

            for (var i = 0; i < coords.Length; i++)
            {
                var x = p1.x + (steps.x * i);
                var y = p1.y + (steps.y * i);

                if (x > 1.0f || x < 0.0f || y > 1.0f || y < 0.0f)
                    coords[i] = new Vector2Int(-1, -1);
                else
                    coords[i] = new Vector2Int((int) (x * frame.width), (int) (y * frame.height));
            }

            return coords;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using DigitalSputnik;
using DigitalSputnik.Colors;
using DigitalSputnik.Voyager;
using UnityEngine;
using VoyagerController.Effects;

namespace VoyagerController.Rendering
{
    internal class InterpolationState : VideoRenderState
    {
        private readonly RenderQueue _queue;
        private readonly VideoEffect _effect;
        private readonly VoyagerLamp[] _lamps;
        private readonly List<ulong> _missingFrames;
        
        public InterpolationState(RenderQueue queue, VideoEffect current)
        {
            _queue = queue;
            _effect = current;
            _lamps = GetLampsWithEffect(_effect).ToArray();
            _missingFrames = GenerateMissingFramesList();
        }
        
        internal override VideoRenderState Update()
        {
            foreach (var voyager in _lamps)
            {
                var meta = Metadata.Get(voyager.Serial);
                
                foreach (var frame in _missingFrames)
                {
                    if (meta.FrameBuffer[frame] != null) continue;

                    var prevIndex = PreviousFrameFromIndex(meta, frame);
                    var nextIndex = NextFrameFromIndex(meta, frame);
                    var time = FrameInterpolationTime(meta, frame);

                    var prevFrame = meta.FrameBuffer[prevIndex];
                    var nextFrame = meta.FrameBuffer[nextIndex];

                    var colors = InterpolateRgb(prevFrame, nextFrame, time);
                    
                    LampEffectsWorker.ApplyVideoFrameToVoyager(voyager, (long) frame, colors);
                }
            }

            if (!_queue.Empty)
                return new RenderState(_queue);
            
            return new DisposeState();
        }

        private ulong PreviousFrameFromIndex(LampMetadata meta, ulong frame)
        {
            var prev = (long) frame - 1;
            while (meta.FrameBuffer[NormalizeFrame(prev)] == null) prev--;
            return NormalizeFrame(prev);
        }

        private ulong NextFrameFromIndex(LampMetadata meta, ulong frame)
        {
            var next = (long) frame + 1;
            while (meta.FrameBuffer[NormalizeFrame(next)] == null) next++;
            return NormalizeFrame(next);
        }

        private ulong NormalizeFrame(long frame)
        {
            var count = (long) _effect.Video.FrameCount;
            if (frame >= count)
                return (ulong) (frame - count);
            if (frame < 0)
                return (ulong) (frame + count);
            return (ulong) frame;
        }

        private float FrameInterpolationTime(LampMetadata meta, ulong frame)
        {
            var prev = (long) frame - 1;
            while (meta.FrameBuffer[NormalizeFrame(prev)] == null) prev--;
            
            var next = (long) frame + 1;
            while (meta.FrameBuffer[NormalizeFrame(next)] == null) next++;

            return (float) (((double) frame - prev) / ((double) next - prev));
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
        
        private static IEnumerable<VoyagerLamp> GetLampsWithEffect(Effect effect)
        {
            return LampManager.Instance
                .GetLampsOfType<VoyagerLamp>()
                .Where(l => Metadata.Get(l.Serial).Effect == effect);
        }

        private static Rgb[] InterpolateRgb(IReadOnlyList<Rgb> a, IReadOnlyList<Rgb> b, float t)
        {
            var result = new Rgb[a.Count];
            for (var i = 0; i < result.Length; i++)
            {
                var red = Mathf.Lerp(a[i].R, b[i].R, t);
                var green = Mathf.Lerp(a[i].G, b[i].G, t);
                var blue = Mathf.Lerp(a[i].B, b[i].B, t);
                result[i] = new Rgb(red, green, blue);
            }
            return result;
        }
    }
}
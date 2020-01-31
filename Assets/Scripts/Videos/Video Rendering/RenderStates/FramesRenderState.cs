using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using VoyagerApp.Lamps;
using VoyagerApp.Utilities;

namespace VoyagerApp.Videos
{
    public class FramesRenderState : RenderState
    {
        const int TRY_COUNT = 3;

        RenderQueue queue;
        long lastMissing;
        bool wait;
        bool render;
        bool next;

        Dictionary<long, int> unable = new Dictionary<long, int>();

        public FramesRenderState(RenderQueue queue)
        {
            this.queue = queue;
            next = true;
        }

        public override RenderState Update()
        {
            if (wait == true)
            {
                wait = false;
                return this;
            }

            var missingFrames = queue.MissingFramesOfActiveVideo;

            if (render)
            {
                long index = VideoRenderer.CurrentFrameIndex;

                if (missingFrames.Any(i => i == index))
                {
                    Texture2D frame = TextureUtils.RenderTextureToTexture2D(VideoRenderer.VideoTexture);

                    foreach (var lamp in queue.LampsWithActiveVideo)
                        RenderFrameToLamp(lamp, frame, index);

                    Object.Destroy(frame);

                    VideoRenderer.UpdateProgress(queue.Progress);

                    next = true;
                    render = false;
                }

                if (GetNextThreePixels(lastMissing).Contains(index))
                {
                    if (unable.ContainsKey(lastMissing))
                    {
                        if (unable[lastMissing] == TRY_COUNT)
                        {
                            foreach (var lamp in queue.LampsWithActiveVideo)
                                RenderUnexistingFrame(lamp, lastMissing);
                            unable.Remove(lastMissing);
                        }
                        else
                            unable[lastMissing]++;
                    }
                    else
                        unable.Add(lastMissing, 1);

                    next = true;
                    render = false;
                }
            }

            if (next)
            {
                if (missingFrames.Length > 0)
                {
                    lastMissing = missingFrames[0];
                    VideoRenderer.Seek(math.clamp(lastMissing - 3, 0, queue.activeVideo.frames));
                    next = false;
                }
            }

            if (missingFrames.Length == 0)
            {
                VideoRenderer.Clear();
                return new ProcessQueueState(queue);
            }

            return this;
        }

        long[] GetNextThreePixels(long index)
        {
            long[] pixels = new long[3];
            for (long i = 1; i <= 3; i++)
            {
                long j = index + i;
                if (j >= queue.activeVideo.frames)
                    j -= queue.activeVideo.frames;
                pixels[i - 1] = j;
            }
            return pixels;
        }

        void RenderFrameToLamp(Lamp lamp, Texture2D frame, long index)
        {
            var coords = VectorUtils.MapLampToVideoCoords(lamp, frame);
            var colors = TextureUtils.CoordsToColors(coords, frame);
            lamp.PushFrame(colors, index);
        }

        void RenderUnexistingFrame(Lamp lamp, long index)
        {
            var prevIndex = GetClosestPrevious(lamp, index, out long p);
            var nextIndex = GetClosestNext(lamp, index, out long n);

            var prevCol = ColorUtils.BytesToColors(lamp.buffer.GetFrame(prevIndex));
            var nextCol = ColorUtils.BytesToColors(lamp.buffer.GetFrame(nextIndex));

            var time = ((float)index - p) / (n - p);

            Debug.Log($"Lerped index {index}, time {time}");
            var lerpedCol = ColorUtils.LerpColorArray(prevCol, nextCol, time);

            lamp.PushFrame(lerpedCol, index);
        }

        long GetClosestNext(Lamp lamp, long index, out long unclamped)
        {
            long i = 1;
            while (true)
            {
                unclamped = index - i;

                long clamped = unclamped;
                if (clamped < 0)
                    clamped += queue.activeVideo.frames;

                if (lamp.buffer.FrameExists(clamped))
                    return clamped;

                i++;
            }
        }

        long GetClosestPrevious(Lamp lamp, long index, out long unclamped)
        {
            long i = 1;
            while (true)
            {
                unclamped = index + i;

                long clamped = unclamped;
                if (clamped >= queue.activeVideo.frames)
                    clamped -= queue.activeVideo.frames;

                if (lamp.buffer.FrameExists(clamped))
                    return clamped;

                i++;
            }
        }

        public override void HandleEvent(VideoRenderEvent type)
        {
            if (type == VideoRenderEvent.Seeked)
            {
                wait = true;
                render = true;
            }
        }
    }
}

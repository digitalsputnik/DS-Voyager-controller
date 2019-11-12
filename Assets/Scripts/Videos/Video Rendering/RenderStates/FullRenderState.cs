using UnityEngine;
using VoyagerApp.Lamps;
using VoyagerApp.Utilities;

namespace VoyagerApp.Videos
{
    public class FullRenderState : RenderState
    {
        bool render;
        RenderQueue queue;
        Video video;
        long prevIndex = -1;
        long rendered = 0;

        public FullRenderState(RenderQueue queue)
        {
            this.queue = queue;
            video = queue.activeVideo;
            VideoRenderer.SetVideo(video);
        }

        public override RenderState Update()
        {
            if (render)
            {
                long index = VideoRenderer.CurrentFrameIndex;

                if (index == prevIndex) return this;

                if (rendered >= queue.activeVideo.frames)
                    return new FramesRenderState(queue);

                Texture2D frame = TextureUtils.RenderTextureToTexture2D(VideoRenderer.VideoTexture);

                foreach (var lamp in queue.LampsWithActiveVideo)
                    RenderFrameToLamp(lamp, frame, index);

                Object.Destroy(frame);

                VideoRenderer.UpdateProgress(queue.Progress);

                prevIndex = index;
                rendered++;
            }

            return this;
        }

        void RenderFrameToLamp(Lamp lamp, Texture2D frame, long index)
        {
            var coords = VectorUtils.MapLampToVideoCoords(lamp, frame);
            var colors = TextureUtils.CoordsToColors(coords, frame);
            lamp.PushFrame(colors, index);
        }

        public override void HandleEvent(VideoRenderEvent type)
        {
            if (type == VideoRenderEvent.Prepared)
                VideoRenderer.Play();

            if (type == VideoRenderEvent.Starting)
            {
                long frame = TimeUtils.GetFrameOfVideo(video, 0.3f);
                VideoRenderer.Seek(frame);
            }

            if (type == VideoRenderEvent.Seeked)
                render = true;
        }
    }
}

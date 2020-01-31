using UnityEngine;
using VoyagerApp.Effects;
using VoyagerApp.Lamps;
using VoyagerApp.UI;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;

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
                VideoRenderer.Seek(GetStartFrame());
            }

            if (type == VideoRenderEvent.Seeked)
                render = true;
        }

        long GetStartFrame()
        {
            if (ApplicationState.Playmode.value == GlobalPlaymode.Play)
                return TimeUtils.GetFrameOfVideo(video, 0.3f);

            VoyagerItemView itemView = null;

            foreach (var item in WorkspaceManager.instance.Items)
            {
                if (item is VoyagerItemView iw)
                    if (queue.LampsWithActiveVideo.Contains(iw.lamp))
                        itemView = iw;
            }

            long frame = itemView.prevFrame - 2;
            if (frame < 0)
                frame += queue.activeVideo.frames;
            return frame;
        }
    }
}

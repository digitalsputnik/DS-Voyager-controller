using UnityEngine;

namespace VoyagerApp.Videos
{
    public class ProcessQueueState : RenderState
    {
        RenderQueue queue;

        public ProcessQueueState(RenderQueue queue)
        {
            this.queue = queue;
        }

        public override RenderState Update()
        {
            queue.ClearRenderedVideos();

            if (queue.activeVideo == null)
                return new ConfirmPixelsState();

            return new FullRenderState(queue);
        }

        public override void HandleEvent(VideoRenderEvent type) { }
    }
}

using VoyagerApp.Utilities;

namespace VoyagerApp.Videos
{
    public class PrepereQueueState : RenderState
    {
        RenderQueue queue;

        public PrepereQueueState()
        {
            queue = new RenderQueue();
            VideoRenderer.UpdateProgress(0.0f);
        }

        public override RenderState Update()
        {
            queue = new RenderQueue();
            foreach (var lamp in WorkspaceUtils.Lamps)
            {
                if (!lamp.buffer.Rendered)
                    queue.AddLamp(lamp);
            }

            queue.PrepereVideoQueue();

            if (queue.videos.Count == 0)
                return new DoneState();

            return new ProcessQueueState(queue);
        }

        public override void HandleEvent(VideoRenderEvent type) { }
    }
}

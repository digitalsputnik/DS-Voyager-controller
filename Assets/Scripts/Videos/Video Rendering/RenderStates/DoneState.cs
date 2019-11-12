using System.Linq;
using VoyagerApp.Utilities;

namespace VoyagerApp.Videos
{
    public class DoneState : RenderState
    {
        public override RenderState Update()
        {
            if (!WorkspaceUtils.Lamps.All(l => l.buffer.Rendered))
                return new PrepereQueueState();
            return this;
        }

        public override void HandleEvent(VideoRenderEvent type) { }
    }
}

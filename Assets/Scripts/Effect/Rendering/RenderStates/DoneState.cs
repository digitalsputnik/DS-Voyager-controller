using System.Linq;
using VoyagerApp.Effects;
using VoyagerApp.Utilities;

namespace VoyagerApp.Videos
{
    public class DoneState : RenderState
    {
        const double CONFIRM_STATE_TIME = 5.0f;

        double _startTime;

        public DoneState()
        {
            _startTime = TimeUtils.Epoch;
        }

        public override RenderState Update()
        {
            if (!WorkspaceUtils.Lamps.Where(l => l.effect is Image).All(l => l.buffer.rendered))
                return new RenderImageState();

            if (!WorkspaceUtils.Lamps.Where(l => l.effect is Video).All(l => l.buffer.rendered))
                return new PrepereQueueState();

            if (TimeUtils.Epoch - _startTime > CONFIRM_STATE_TIME)
                return new ConfirmPixelsState();

            return this;
        }

        public override void HandleEvent(VideoRenderEvent type) { }
    }
}

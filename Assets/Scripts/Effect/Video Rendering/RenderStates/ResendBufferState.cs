using System.Collections.Generic;
using System.Linq;
using VoyagerApp.Effects;
using VoyagerApp.Lamps;
using VoyagerApp.UI;
using VoyagerApp.Utilities;

namespace VoyagerApp.Videos
{
    public class ResendBufferState : RenderState
    {
        readonly List<LampState> states = new List<LampState>();
        readonly List<Lamp> renderedLamps = new List<Lamp>();

        public void AddLamp(Lamp lamp)
        {
            LampState state = states.FirstOrDefault(s => s.lamp == lamp);

            if (state != null)
                states.Remove(state);

            if (renderedLamps.Contains(lamp))
                renderedLamps.Remove(lamp);

            var start = GetStartFrame(lamp, 0);

            state = new LampState(lamp, start);

            states.Add(state);
        }

        public override void HandleEvent(VideoRenderEvent type)
        {
            
        }

        public override RenderState Update()
        {
            foreach (var state in states.ToArray())
            {
                var frame = state.frame;
                var lamp = state.lamp;

                var data = lamp.buffer.GetFrame(frame);
                var colors = ColorUtils.BytesToColors(data);
                lamp.PushFrame(colors, frame);

                frame++;

                if (frame >= lamp.buffer.count)
                    frame -= lamp.buffer.count;

                if (frame == state.start)
                {
                    renderedLamps.Add(state.lamp);
                    states.Remove(state);
                    continue;
                }

                state.frame = frame;
                state.done++;
            }

            if (states.Count == 0)
            {
                RenderQueue queue = new RenderQueue();
                foreach (var lamp in renderedLamps)
                    queue.AddLamp(lamp);
                queue.PrepereVideoQueue();
                return new ProcessQueueState(queue);
            }

            VideoRenderer.UpdateProgress(states.Sum(s => s.processed) / states.Count);

            return this;
        }

        long GetStartFrame(Lamp lamp, long offset)
        {
            Video video = (Video)lamp.effect;

            if (ApplicationState.Playmode.value == GlobalPlaymode.Play)
                return TimeUtils.GetFrameOfVideo(video, 0.5);

            var itemView = WorkspaceUtils.SelectedVoyagerLampItems.FirstOrDefault(i => i.lamp == lamp);

            long frame = itemView.prevFrame + offset;
            if (frame < 0)
                frame += video.frames;

            return frame;
        }

        class LampState
        {
            public Lamp lamp;
            public long start;
            public long frame;
            public long done;
            public float processed => (float)done / lamp.buffer.count;

            public LampState(Lamp lamp, long start)
            {
                this.lamp = lamp;
                this.start = start;
                frame = start;
                done = 0;
            }
        }
    }
}

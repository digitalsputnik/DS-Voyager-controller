using System.Collections.Generic;
using System.Linq;
using VoyagerApp.Effects;
using VoyagerApp.Lamps;

namespace VoyagerApp.Videos
{
    public class RenderQueue
    {
        public List<Lamp> lamps = new List<Lamp>();
        public Queue<Video> videos = new Queue<Video>();
        public Video activeVideo = new Video();

        public void AddLamp(Lamp lamp)
        {
            if (!lamps.Contains(lamp))
                lamps.Add(lamp);
        }

        public void PrepereVideoQueue()
        {
            foreach (var lamp in lamps)
            {
                if (lamp.effect is Video video)
                {
                    if (!videos.Contains(video) && video.width != 0)
                        videos.Enqueue(video);
                }
            }
        }

        public void ClearRenderedVideos()
        {
            foreach (var lamp in lamps.ToArray())
            {
                if (lamp.buffer.rendered || !(lamp.effect is Video))
                    lamps.Remove(lamp);
            }

            videos.Clear();
            PrepereVideoQueue();

            if (videos.Count > 0)
                activeVideo = videos.Dequeue();
            else
                activeVideo = null;
        }

        public List<Lamp> LampsWithActiveVideo
        {
            get
            {
                if (activeVideo == null) return null;
                return lamps.Where(l => l.effect == activeVideo).ToList();
            }
        }

        public long[] MissingFramesOfActiveVideo
        {
            get
            {
                List<long> frames = new List<long>();

                foreach (var lamp in LampsWithActiveVideo)
                {
                    for (long i = 0; i < lamp.buffer.count; i++)
                    {
                        if (!lamp.buffer.FrameExists(i) && !frames.Contains(i))
                            frames.Add(i);
                    }
                }

                return frames.ToArray();
            }
        }

        public float Progress
        {
            get
            {
                long all = LampManager.instance.Lamps.Sum(l => l.buffer.count);
                long done = LampManager.instance.Lamps.Sum(l => l.buffer.existing);
                return (float)done / all;
            }
        }
    }
}

using System.Collections.Generic;
using System.Linq;
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
                if (!videos.Contains(lamp.video) && lamp.video.width != 0)
                    videos.Enqueue(lamp.video);
            }
        }

        public void ClearRenderedVideos()
        {
            foreach (var lamp in lamps.ToArray())
            {
                if (lamp.buffer.Rendered)
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
                return lamps.Where(l => l.video == activeVideo).ToList();
            }
        }

        public long[] MissingFramesOfActiveVideo
        {
            get
            {
                List<long> frames = new List<long>();

                foreach (var lamp in LampsWithActiveVideo)
                {
                    for (long i = 0; i < lamp.buffer.frames; i++)
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
                long all = LampManager.instance.Lamps.Sum(l => l.buffer.frames);
                long done = LampManager.instance.Lamps.Sum(l => l.buffer.ExistingFramesCount);
                return (float)done / all;
            }
        }
    }
}

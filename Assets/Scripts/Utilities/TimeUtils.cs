using System;
using VoyagerApp.Videos;

namespace VoyagerApp.Utilities
{
    public static class TimeUtils
    {
        public static double Epoch
        {
            get => (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static long GetFrameOfVideo(Video video, double offset = 0.0f)
        {
            var since = Epoch - video.lastStartTime + offset;
            var frames = (long)(since * video.fps);

            while (frames < 0)
                frames += video.frames;

            return frames % video.frames;
        }

        public static double GetTimeOfVideo(Video video, double offset = 0.0f)
        {
            return ((Epoch - video.lastStartTime) % video.duraction) + offset;
        }
    }
}
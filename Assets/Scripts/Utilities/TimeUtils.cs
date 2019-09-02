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

        public static long GetFrameOfVideo(Video video)
        {
            var since = Epoch - video.lastStartTime;
            var frames = (long)(since * video.fps);
            return frames % video.frames;
        }
    }
}
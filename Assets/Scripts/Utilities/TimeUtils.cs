using System;
using Unity.Mathematics;
using VoyagerApp.Effects;

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
            var since = Epoch - video.startTime + offset;

            var frames = (long)(since * video.fps);

            while (frames < 0)
                frames += video.frames;

            return frames % video.frames;
        }

        public static double GetTimeOfVideo(Video video, double offset = 0.0f)
        {
            return ((Epoch - video.startTime) % video.duraction) + offset;
        }

        public static string GetVideoTimecode(Video video)
        {
            var time = TimeSpan.FromSeconds(video.duraction);
            var frames = (int)math.round((float)time.Milliseconds / 1000 * video.fps);
            return time.ToString(@"hh\:mm\:ss\:") + frames;
        }
    }
}
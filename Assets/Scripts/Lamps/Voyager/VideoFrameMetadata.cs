using System;
using Newtonsoft.Json;
using VoyagerApp.Utilities;
using VoyagerApp.Videos;

namespace VoyagerApp.Lamps.Voyager
{
    [Serializable]
    public class VideoFrameMetadata : JsonData<VideoFrameMetadata>
    {
        [JsonProperty("itshe")]
        public Itshe itshe;
        [JsonProperty("fps")]
        public float fps;
        [JsonProperty("frame_count")]
        public float frames;
        [JsonProperty("video_starttime")]
        public double videoStartTime;
        [JsonProperty("video_timestamp")]
        public double videoTimestamp;
        [JsonProperty("timestamp")]
        public double timestamp;

        public VideoFrameMetadata(Itshe itshe, float fps, float frames,
                                  double timestamp, double videoStartTime,
                                  double videoTimestamp)
        {
            this.itshe = itshe;
            this.fps = fps;
            this.frames = frames;
            this.videoStartTime = videoStartTime;
            this.videoTimestamp = videoTimestamp;
            this.timestamp = timestamp;
        }

        public static VideoFrameMetadata FromVideo(Video video, Itshe itshe,
                                                   double timestamp,
                                                   double offset)
        {
            timestamp += offset;

            if (video == null)
                return new VideoFrameMetadata(itshe, 0, 0, timestamp, 0, 0);

            double vStart = video.lastStartTime + offset;
            double vTimestamp = video.lastTimestamp + offset;

            return new VideoFrameMetadata(itshe, video.fps, video.frames,
                                          timestamp, vStart, vTimestamp);
        }
    }
}
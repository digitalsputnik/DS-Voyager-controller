using System;
using Newtonsoft.Json;
using UnityEngine;
using VoyagerApp.Utilities;

namespace VoyagerApp.Lamps.Voyager
{
    [Serializable]
    public class VideoFrameData : JsonData<VideoFrameData>
    {
        [JsonProperty("frame_index")]
        public long frame;
        [JsonProperty("frame_array")]
        public byte[] data;

        public static VideoFrameData FromColors(long frame, Color32[] colors)
        {
            return new VideoFrameData
            {
                frame = frame,
                data = ColorUtils.ColorsToBytes(colors)
            };
        }
    }
}
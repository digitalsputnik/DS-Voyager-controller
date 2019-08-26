using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using VoyagerApp.Utilities;

namespace VoyagerApp.Lamps.Voyager
{
    [Serializable]
    public class VoyagerPlaybackPackage : JsonData<VoyagerPlaybackPackage>
    {
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("Playmode")]
        public VoyagerPlaybackMode playmode;
        [JsonProperty("Timestamp")]
        public double timestamp;

        public VoyagerPlaybackPackage(VoyagerPlaybackMode mode, double time)
        {
            playmode = mode;
            timestamp = time;
        }
    }

    [Serializable]
    public enum VoyagerPlaybackMode
    {
        Play,
        Pause,
        Stop
    }
}
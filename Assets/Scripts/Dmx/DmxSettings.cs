using System;
using Newtonsoft.Json;
using VoyagerApp.Utilities;

namespace VoyagerApp.Dmx
{
    [Serializable]
    public class DmxSettings : JsonData<DmxSettings>
    {
        [JsonProperty("DMXmode")]
        public bool enabled;
        [JsonProperty("Universe")]
        public int universe;
        [JsonProperty("Channel")]
        public int channel;
        [JsonProperty("Division")]
        public int division;
        [JsonProperty("Protocol")]
        public DmxProtocol protocol;
        [JsonProperty("Format")]
        public DmxFormat format;
    }

    [Serializable]
    public class DmxPoll : JsonData<DmxPoll>
    {
        [JsonProperty("poll_dmx")]
        public bool poll = true;
    }
}
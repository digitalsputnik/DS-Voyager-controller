using System;
using Newtonsoft.Json;
using VoyagerApp.Dmx;

namespace VoyagerApp.Networking.Voyager
{
    [Serializable]
    public class DmxModeResponse : Packet
    {
        [JsonProperty("enabled")]
        public bool enabled;
        [JsonProperty("universe")]
        public int universe;
        [JsonProperty("channel")]
        public int channel;
        [JsonProperty("division")]
        public int division;
        [JsonProperty("protocol")]
        public string protocol;
        [JsonProperty("pixel_format")]
        public string format;

        public DmxModeResponse() : base(OpCode.SetDmxMode) { }

        public DmxModeResponse(bool enabled, int universe, int channel, int division, DmxProtocol protocol, DmxFormat format) : this()
        {
            this.enabled = enabled;
            this.universe = universe;
            this.channel = channel;
            this.division = division;
            this.protocol = protocol.ToString();
            this.format = format.ToString().ToLower();
        }
    }
}
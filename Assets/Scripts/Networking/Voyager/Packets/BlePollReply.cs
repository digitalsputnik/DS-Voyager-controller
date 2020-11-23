using Newtonsoft.Json;

namespace VoyagerApp.Networking.Voyager
{
    public class BlePollReply : Packet
    {
        [JsonProperty("length")]
        public int Length { get; set; }
        [JsonProperty("battery_level")]
        public int Battery { get; set; }
        [JsonProperty("serial_name")]
        public string Serial { get; set; }
        [JsonProperty("IP")]
        public byte[] IpAddress { get; set; }
        [JsonProperty("CHIP_version")]
        public int[] Version { get; set; }
        
        public BlePollReply() : base(OpCode.PollReply) { }
    }
}
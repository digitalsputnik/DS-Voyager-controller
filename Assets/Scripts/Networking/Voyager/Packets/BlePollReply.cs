using Newtonsoft.Json;

namespace VoyagerApp.Networking.Voyager
{
    public class BlePollReply : Packet
    {
        [JsonProperty("op_code")]
        public string Op { get; set; }
        [JsonProperty("length")] 
        public int Length { get; set; }
        [JsonProperty("battery_level")]
        public int Battery { get; set; }
        [JsonProperty("serial")]
        public string Serial { get; set; }
        [JsonProperty("ip")]
        public byte[] IpAddress { get; set; }
        [JsonProperty("version")]
        public int[] Version { get; set; }
        
        public BlePollReply() : base(OpCode.PollReply) { }
    }
}
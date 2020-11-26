using Newtonsoft.Json;

namespace VoyagerApp.Networking.Voyager
{
    public class BlePollReply : Packet
    {
        [JsonProperty("op_code")]
        public string Op { get; set; }
        [JsonProperty("len")] 
        public int Length { get; set; }
        [JsonProperty("bl")]
        public int Battery { get; set; }
        [JsonProperty("sn")]
        public string Serial { get; set; }
        [JsonProperty("IP")]
        public byte[] IpAddress { get; set; }
        [JsonProperty("cv")]
        public int[] Version { get; set; }
        
        public BlePollReply() : base(OpCode.PollReply) { }
    }
}
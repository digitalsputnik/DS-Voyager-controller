using Newtonsoft.Json;

namespace VoyagerApp.Networking.Packages.Voyager
{
    public class SetVideoPacket : Packet
    {
        [JsonProperty("frame_count")]
        public long frameCount;
        [JsonProperty("start_time")]
        public double startTime;

        public SetVideoPacket() : base(OpCode.SetVideo) { }

        public SetVideoPacket(long frameCount, double startTime) : this()
        {
            this.frameCount = frameCount;
            this.startTime = startTime;
        }
    }
}
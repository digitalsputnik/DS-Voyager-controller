using Newtonsoft.Json;

namespace VoyagerApp.Networking.Packages.Voyager
{
    public class SetFramePacket : Packet
    {
        [JsonProperty("index")]
        public long index;
        [JsonProperty("frame")]
        public byte[] frame;

        public SetFramePacket() : base(OpCode.SetFrame) { }

        public SetFramePacket(long index, byte[] frame) : this()
        {
            this.index = index;
            this.frame = frame;
        }
    }
}
using Newtonsoft.Json;

namespace VoyagerApp.Networking.Packages.Voyager
{
    public class MissingFramesResponsePacket : Packet
    {
        [JsonProperty("indices")]
        public long[] indices;

        public MissingFramesResponsePacket() : base(OpCode.MissingFramesResponse) { }

        public MissingFramesResponsePacket(long[] indices) : this()
        {
            this.indices = indices;
        }
    }
}
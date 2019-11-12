using Newtonsoft.Json;

namespace VoyagerApp.Networking.Voyager
{
    public class FpsResponsePacket : Packet
    {
        [JsonProperty("fps")]
        public int Fps;

        public FpsResponsePacket() : base(OpCode.FpsResponse) { }
    }
}
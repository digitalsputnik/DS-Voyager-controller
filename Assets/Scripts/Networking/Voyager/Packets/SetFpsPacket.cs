using Newtonsoft.Json;

namespace VoyagerApp.Networking.Voyager
{
    public class SetFpsPacket : Packet
    {
        [JsonProperty("fps")]
        public int Fps;

        public SetFpsPacket() : base(OpCode.SetFps) { }

        public SetFpsPacket(int fps) : this()
        {
            Fps = fps;
        }
    }
}
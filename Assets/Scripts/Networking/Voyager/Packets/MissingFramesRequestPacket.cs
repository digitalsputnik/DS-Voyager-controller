using Newtonsoft.Json;

namespace VoyagerApp.Networking.Voyager
{
    public class MissingFramesRequestPacket : Packet
    {
        [JsonProperty("video_uid")]
        public double videoTimestamp;

        public MissingFramesRequestPacket(double videoTimestamp) : base(OpCode.MissingFramesRequest)
        {
            this.videoTimestamp = videoTimestamp;
        }
    }
}
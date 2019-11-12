using System;

namespace VoyagerApp.Networking.Voyager
{
    [Serializable]
    public class VideoRequestPacket : Packet
    {
        public VideoRequestPacket() : base(OpCode.VideoRequest) { }
    }
}
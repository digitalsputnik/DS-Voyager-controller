using System;

namespace VoyagerApp.Networking.Packages.Voyager
{
    [Serializable]
    public class VideoRequestPacket : Packet
    {
        public VideoRequestPacket() : base(OpCode.VideoRequest) { }
    }
}
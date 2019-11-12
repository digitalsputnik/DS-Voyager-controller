using System;

namespace VoyagerApp.Networking.Voyager
{
    [Serializable]
    public class SsidListRequestPacket : Packet
    {
        public SsidListRequestPacket() : base(OpCode.SsidListRequest) { }
    }
}
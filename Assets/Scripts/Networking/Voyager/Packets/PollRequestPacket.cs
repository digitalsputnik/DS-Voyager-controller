using System;

namespace VoyagerApp.Networking.Voyager
{
    [Serializable]
    public class PollRequestPacket : Packet
    {
        public PollRequestPacket() : base(OpCode.PollRequest) { }
    }
}
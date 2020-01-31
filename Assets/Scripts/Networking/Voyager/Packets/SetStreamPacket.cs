using System;

namespace VoyagerApp.Networking.Voyager
{
    [Serializable]
    public class SetStreamPacket : Packet
    {
        public SetStreamPacket() : base(OpCode.SetStream) { }
    }
}
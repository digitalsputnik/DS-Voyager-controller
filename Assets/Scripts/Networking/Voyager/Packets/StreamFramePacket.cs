using System;

namespace VoyagerApp.Networking.Voyager
{
    [Serializable]
    public class StreamFramePacket : Packet
    {
        public double index;
        public byte[] frame;

        public StreamFramePacket() : base(OpCode.StreamFrame) { }

        public StreamFramePacket(double index, byte[] frame) : this()
        {
            this.index = index;
            this.frame = frame;
        }
    }
}
namespace VoyagerApp.Networking.Voyager
{
    public class FpsRequestPacket : Packet
    {
        public FpsRequestPacket() : base(OpCode.FpsRequest) { }
    }
}
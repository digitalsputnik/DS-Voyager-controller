namespace VoyagerApp.Networking.Packages.Voyager
{
    public class FpsRequestPacket : Packet
    {
        public FpsRequestPacket() : base(OpCode.FpsRequest) { }
    }
}
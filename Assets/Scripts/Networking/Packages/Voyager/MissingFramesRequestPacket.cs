namespace VoyagerApp.Networking.Packages.Voyager
{
    public class MissingFramesRequestPacket : Packet
    {
        public MissingFramesRequestPacket() : base(OpCode.MissingFramesRequest) { }
    }
}
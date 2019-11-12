namespace VoyagerApp.Networking.Voyager
{
    public class MissingFramesRequestPacket : Packet
    {
        public MissingFramesRequestPacket() : base(OpCode.MissingFramesRequest) { }
    }
}
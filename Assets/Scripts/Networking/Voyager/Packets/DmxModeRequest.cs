using System;

namespace VoyagerApp.Networking.Voyager
{
    [Serializable]
    public class DmxModeRequest : Packet
    {
        public DmxModeRequest() : base(OpCode.DmxModeRequest) { }
    }
}
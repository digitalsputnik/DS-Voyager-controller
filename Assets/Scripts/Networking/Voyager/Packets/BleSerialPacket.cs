using System;

namespace VoyagerApp.Networking.Voyager
{
    [Serializable]
    public class BleSerialPacket : Packet
    {
        public BleSerialPacket() : base(OpCode.GetSerial) { }
    }
}
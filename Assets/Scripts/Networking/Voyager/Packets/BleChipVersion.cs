using System;

namespace VoyagerApp.Networking.Voyager
{
    [Serializable]
    public class BleChipVersion : Packet
    {
        public int[] version;

        public BleChipVersion() : base(OpCode.GetChipVersion) { }
    }
}
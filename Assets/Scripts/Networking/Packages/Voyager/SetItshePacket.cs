using System;
using Newtonsoft.Json;

namespace VoyagerApp.Networking.Packages.Voyager
{
    [Serializable]
    public class SetItshePacket : Packet
    {
        [JsonProperty("itshe")]
        public Itshe itshe;

        public SetItshePacket() : base(OpCode.SetItshe) { }

        public SetItshePacket(Itshe itshe) : this() => this.itshe = itshe;
    }
}
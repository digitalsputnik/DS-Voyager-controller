using System;
using Newtonsoft.Json;

namespace VoyagerApp.Networking.Packages
{
    [Serializable]
    public class PacketCollection : Packet
    {
        [JsonProperty("packets")]
        public Packet[] packets;

        public PacketCollection(params Packet[] packets)
            : base(OpCode.Collection) => this.packets = packets;
    }
}
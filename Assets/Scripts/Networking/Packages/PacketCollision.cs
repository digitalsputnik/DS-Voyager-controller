using System;
using Newtonsoft.Json;

namespace VoyagerApp.Networking.Packages
{
    [Serializable]
    public class PacketCollision : Packet
    {
        [JsonProperty("packets")]
        public Packet[] packets;

        public PacketCollision(params Packet[] packets)
            : base(OpCode.Collection) => this.packets = packets;
    }
}
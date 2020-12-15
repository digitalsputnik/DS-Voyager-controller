using System;
using Newtonsoft.Json;
using UnityEngine;
using VoyagerApp.Utilities;

namespace VoyagerApp.Networking.Voyager
{
    [Serializable]
    public class SetFramePacket : Packet
    {
        [JsonProperty("index")]
        public long index;
        [JsonProperty("frame")]
        public byte[] frame;

        public SetFramePacket() : base(OpCode.SetFrame) { }

        public SetFramePacket(long index, byte[] frame) : this()
        {
            this.frame = frame;
            this.index = index;
        }
    }
}
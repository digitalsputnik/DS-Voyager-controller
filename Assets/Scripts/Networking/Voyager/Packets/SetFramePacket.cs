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
        [JsonProperty("temperature")]
        public float temperature;
        [JsonProperty("frame")]
        public byte[] frame;

        public SetFramePacket() : base(OpCode.SetFrame) { }

        public SetFramePacket(long index, Itshe itshe, byte[] frame) : this()
        {
            this.frame = frame;
            this.index = index;
            temperature = itshe.t;
        }
    }
}
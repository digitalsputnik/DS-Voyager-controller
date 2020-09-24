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
            Color32[] colors = ColorUtils.BytesToColors(frame);
            colors = ColorUtils.MixColorsToItshe(colors, itshe);
            this.frame = ColorUtils.ColorsToBytes(colors);
            this.index = index;
            temperature = itshe.t;
        }
    }
}
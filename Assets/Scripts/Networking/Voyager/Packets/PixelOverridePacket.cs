using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace VoyagerApp.Networking.Voyager
{
    public class PixelOverridePacket : Packet
    {
        [JsonProperty("override")]
        public Dictionary<int, int[]> dictionary = new Dictionary<int, int[]>();
        [JsonProperty("timeout")]
        public double timeout;

        public PixelOverridePacket() : base(OpCode.PixelOverride) { }

        public PixelOverridePacket(Itshe itshe, double timeout) : this()
        {
            Color32 color = itshe.AsColor;
            dictionary.Add(-1, new int[]
            {
                color.r,
                color.g,
                color.b,
                (int)(itshe.t * 8500 + 1500)
            });
            this.timeout = timeout;
        }
    }
}
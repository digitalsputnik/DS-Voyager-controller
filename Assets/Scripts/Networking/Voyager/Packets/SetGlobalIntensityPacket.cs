using System;
using Newtonsoft.Json;

namespace VoyagerApp.Networking.Voyager
{
    [Serializable]
    public class SetGlobalIntensityPacket : Packet
    {
        [JsonProperty("intensity")]
        public float intensity;

        public SetGlobalIntensityPacket() : base(OpCode.SetGlobalIntensity) { }

        public SetGlobalIntensityPacket(float intensity) : this()
        {
            this.intensity = intensity;
        }
    }
}
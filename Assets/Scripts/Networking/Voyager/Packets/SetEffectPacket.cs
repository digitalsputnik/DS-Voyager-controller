using System;
using Newtonsoft.Json;

namespace VoyagerApp.Networking.Voyager
{
    [Serializable]
    public class SetEffectPacket : Packet
    {
        [JsonProperty("effect")]
        public float effect;

        public SetEffectPacket() : base(OpCode.SetEffect) { }

        public SetEffectPacket(float effect) : this() => this.effect = effect;
    }
}
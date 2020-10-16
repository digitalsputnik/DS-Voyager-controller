using DigitalSputnik.Colors;
using Newtonsoft.Json;

namespace VoyagerController.Bluetooth
{
    internal struct SetItshePacket
    {
        [JsonProperty("op_code")]
        public string OpCode { get; set; }
        [JsonProperty("timestamp")]
        public double Timestamp { get; set; }
        [JsonProperty("itshe")]
        public Itshe Itshe { get; set; }

        public SetItshePacket(string op, double timestamp, Itshe itshe)
        {
            OpCode = op;
            Timestamp = timestamp;
            Itshe = itshe;
        }
    }
}
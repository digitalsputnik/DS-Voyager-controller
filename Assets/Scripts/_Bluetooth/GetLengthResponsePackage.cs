using System;
using Newtonsoft.Json;

namespace VoyagerController.Bluetooth
{
    [Serializable]
    internal struct GetLengthResponsePackage
    {
        [JsonProperty("op_code")]
        public string OpCode { get; set; }
        [JsonProperty("length")]
        public int Length { get; set; }
    }
}
using System;
using Newtonsoft.Json;

namespace VoyagerController.Bluetooth
{
    [Serializable]
    internal struct GetSerialResponsePackage
    {
        [JsonProperty("op_code")]
        public string OpCode { get; set; }
        [JsonProperty("serial")]
        public string Serial { get; set; }
    }
}
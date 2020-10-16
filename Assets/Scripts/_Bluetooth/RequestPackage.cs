using System;
using Newtonsoft.Json;

namespace VoyagerController.Bluetooth
{
    [Serializable]
    internal struct RequestPackage
    {
        [JsonProperty("op_code")]
        public string OpCode { get; set; }

        public RequestPackage(string op) => OpCode = op;
    }
}
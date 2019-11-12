using System;
using Newtonsoft.Json;

namespace VoyagerApp.Networking.Voyager
{
    [Serializable]
    public class SsidListResponseResponse : Packet
    {
        [JsonProperty("list")]
        public string[] ssids;

        public SsidListResponseResponse() : base(OpCode.SsidListResponse) { }
    }
}
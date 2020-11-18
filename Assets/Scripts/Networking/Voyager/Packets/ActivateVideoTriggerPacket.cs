using System;
using Newtonsoft.Json;
using VoyagerApp.Utilities;

namespace VoyagerApp.Networking.Voyager
{
    [Serializable]
    public class ActivateVideoTrigger : JsonData<ActivateVideoTrigger>
    {
        [JsonProperty("serial")]
        public string serial { get; set; }

        [JsonProperty("keyword")]
        public string Keyword;
    }
}
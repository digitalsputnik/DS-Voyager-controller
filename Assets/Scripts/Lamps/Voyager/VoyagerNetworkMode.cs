using System;
using Newtonsoft.Json;
using VoyagerApp.Utilities;

namespace VoyagerApp.Lamps.Voyager
{
    [Serializable]
    public class VoyagerNetworkMode : JsonData<VoyagerNetworkMode>
    {
        [JsonProperty("network_mode")]
		public string mode;
        [JsonProperty("set_pattern")]
		public string ssid;
        [JsonProperty("set_pattern_ps")]
        public string password;
        [JsonProperty("serial_name")]
        public string serial;

        public static VoyagerNetworkMode Client(string ssid, string pw, string serial)
        {
            return new VoyagerNetworkMode
            {
                mode = "client_mode",
                ssid = ssid,
                password = pw,
                serial = serial
            };
        }

        public static VoyagerNetworkMode Router(string serial)
        {
            return new VoyagerNetworkMode
            {
                mode = "router_mode",
                serial = serial
            };
        }

        public static VoyagerNetworkMode Master(string serial)
        {
            return new VoyagerNetworkMode
            {
                mode = "ap_mode",
                serial = serial
            };
        }
    }
}
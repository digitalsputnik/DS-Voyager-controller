﻿using System;
using Newtonsoft.Json;
using VoyagerApp.Utilities;

namespace VoyagerApp.Lamps.Voyager
{
    [Serializable]
    public class VoyagerNetworkMode : JsonData<VoyagerNetworkMode>
    {
        [JsonProperty("op_code")]
        public string opcode = "network_mode_request";
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

        public static VoyagerNetworkMode SecureClient(string ssid, string pw, string serial)
        {
            return new VoyagerNetworkMode
            {
                mode = "client_mode_psk",
                ssid = ssid,
                password = (pw.Length == 0) ? "" : SecurityUtility.WPA_PSK(ssid, pw),
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
using System;
using Newtonsoft.Json;
using VoyagerApp.Utilities;

namespace VoyagerApp.Lamps.Voyager
{
    [Serializable]
    public class VoyagerLampInfoResponse : JsonData<VoyagerLampInfoResponse>
    {
        [JsonProperty("IP")]
        public byte[] ip { get; set; }
        [JsonProperty("length")]
        public int length { get; set; }
        [JsonProperty("battery_level")]
        public int battery { get; set; }
        [JsonProperty("BQ_temp")]
        public int[] bqTemperature { get; set; }
        [JsonProperty("CHIP_temp")]
        public int chipTemperature { get; set; }
        [JsonProperty("charging_status")]
        public int[] chargingStatus { get; set; }
        [JsonProperty("LPC_version")]
        public int[] lpcVersion { get; set; }
        [JsonProperty("CHIP_version")]
        public int[] chipVersion { get; set; }
        [JsonProperty("animation_version")]
        public int[] animVersion { get; set; }
        [JsonProperty("MAC_last6")]
        public string macLastDigits { get; set; }
        [JsonProperty("passive_active_mode")]
        public string passiveActiveMode { get; set; }
        [JsonProperty("serial_name")]
        public string serial { get; set; }
        [JsonProperty("hardware_version")]
        public int hwVersion { get; set; }
        [JsonProperty("active_mode")]
        public string activeMode { get; set; }
        [JsonProperty("active_pattern")]
        public string activePattern { get; set; }
        [JsonProperty("active_pattern_ps")]
        public string activePatternPassword { get; set; }
        [JsonProperty("active_channel")]
        public int activeChannel { get; set; }
        [JsonProperty("active_ssid")]
        public string activeSsid { get; set; }
        [JsonProperty("active_password")]
        public string activePassword { get; set; }
    }
}
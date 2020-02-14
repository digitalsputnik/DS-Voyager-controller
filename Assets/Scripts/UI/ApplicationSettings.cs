using Newtonsoft.Json;
using UnityEngine;

namespace VoyagerApp.UI
{
    public static class ApplicationSettings
    {
        public const string HELP_URL = @"https://global.digitalsputnik.com/pages/support_global";
        public const double PLAYBACK_OFFSET = 0.15;

        const string ITSH_IDENTIFICATION  = "identify_itsh";
        const string ITSH_DEFAULT         = "default_itsh";

        const string INFO_BATTERY_LEVEL    = "info_battery_level";
        const string INFO_CHARGING_STATUS  = "info_charging_status";
        const string INFO_WIFI_MODE        = "info_wifi_mode";
        const string INFO_LENGHT           = "info_lenght";
        const string INFO_DMX_UNIVERSE     = "info_dmx_universe";
        const string INFO_DMX_CHANNEL      = "info_dmx_channel";
        const string INFO_IP_ADDRESS       = "info_ip_address";
        const string INFO_FIRMWARE_VERSION = "info_firmware_version";

        public static Itshe IdentificationColor
        {
            get
            {
                Itshe itshe;
                if (PlayerPrefs.HasKey(ITSH_IDENTIFICATION))
                {
                    string json = PlayerPrefs.GetString(ITSH_IDENTIFICATION);
                    itshe = JsonConvert.DeserializeObject<Itshe>(json);
                }
                else
                    itshe = new Itshe(Color.red, 1.0f);

                return itshe;
            }
            set
            {
                string json = JsonConvert.SerializeObject(value);
                PlayerPrefs.SetString(ITSH_IDENTIFICATION, json);
            }
        }

        public static Itshe AddedLampsDefaultColor
        {
            get
            {
                Itshe itshe;
                if (PlayerPrefs.HasKey(ITSH_DEFAULT))
                {
                    string json = PlayerPrefs.GetString(ITSH_DEFAULT);
                    itshe = JsonConvert.DeserializeObject<Itshe>(json);
                }
                else
                {
                    itshe = new Itshe(Color.white, 1.0f);
                    itshe.i = 0.5f;
                }

                return itshe;
            }
            set
            {
                string json = JsonConvert.SerializeObject(value);
                PlayerPrefs.SetString(ITSH_DEFAULT, json);
            }
        }

        public static bool ShowInfoBatteryLevel
        {
            get => GetBool(INFO_BATTERY_LEVEL, true);
            set => SetBool(INFO_BATTERY_LEVEL, value);
        }

        public static bool ShowInfoChargingStatus
        {
            get => GetBool(INFO_CHARGING_STATUS, false);
            set => SetBool(INFO_CHARGING_STATUS, value);
        }

        public static bool ShowInfoWifiMode
        {
            get => GetBool(INFO_WIFI_MODE, true);
            set => SetBool(INFO_WIFI_MODE, value);
        }

        public static bool ShowInfoLenght
        {
            get => GetBool(INFO_LENGHT, true);
            set => SetBool(INFO_LENGHT, value);
        }

        public static bool ShowInfoDmxUniverse
        {
            get => GetBool(INFO_DMX_UNIVERSE, true);
            set => SetBool(INFO_DMX_UNIVERSE, value);
        }

        public static bool ShowInfoDmxChannel
        {
            get => GetBool(INFO_DMX_CHANNEL, true);
            set => SetBool(INFO_DMX_CHANNEL, value);
        }

        public static bool ShowInfoIpAddress
        {
            get => GetBool(INFO_IP_ADDRESS, true);
            set => SetBool(INFO_IP_ADDRESS, value);
        }

        public static bool ShowInfoFirmwareVersion
        {
            get => GetBool(INFO_FIRMWARE_VERSION, false);
            set => SetBool(INFO_FIRMWARE_VERSION, value);
        }

        static void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }

        static bool GetBool(string key, bool defaultValue)
        {
            if (!PlayerPrefs.HasKey(key))
                return defaultValue;
            return PlayerPrefs.GetInt(key) == 1;
        }
    }
}
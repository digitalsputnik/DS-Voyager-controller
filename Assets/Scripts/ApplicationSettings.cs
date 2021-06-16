using DigitalSputnik.Colors;
using Newtonsoft.Json;
using UnityEngine;

namespace VoyagerController
{
    public static class ApplicationSettings
    {
        public const string HELP_URL = @"https://www.digitalsputnik.com/pages/support";
        public const double PLAYBACK_OFFSET = 0.15;

        private const string ITSH_IDENTIFICATION  = "identify_itsh";
        private const string ITSH_DEFAULT         = "default_itsh";

        private const string INFO_BATTERY_LEVEL    = "info_battery_level";
        private const string INFO_CHARGING_STATUS  = "info_charging_status";
        private const string INFO_WIFI_MODE        = "info_wifi_mode";
        private const string INFO_LENGHT           = "info_lenght";
        private const string INFO_DMX_UNIVERSE     = "info_dmx_universe";
        private const string INFO_DMX_CHANNEL      = "info_dmx_channel";
        private const string INFO_IP_ADDRESS       = "info_ip_address";
        private const string INFO_FIRMWARE_VERSION = "info_firmware_version";

        private const string SETT_IOS_BLE_WIFI_SSID = "sett_ios_ble_wifissid";

        private const string SET_CLIENT_SSID = "set_client_ssid";
        private const string SET_CLIENT_PASSWORD = "set_client_password";

        private const string AUTO_LOAD = "auto_load";
        private const string AUTO_SAVE = "auto_save";

        private const string AUTO_SAVE_DELAY = "auto_save_delay";

        public static Itshe IdentificationColor
        {
            get
            {
                var itshe = new Itshe { S = 1.0f, E = 1.0f };

                if (!PlayerPrefs.HasKey(ITSH_IDENTIFICATION)) return itshe;
                
                var json = PlayerPrefs.GetString(ITSH_IDENTIFICATION);
                itshe = JsonConvert.DeserializeObject<Itshe>(json);

                return itshe;
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                PlayerPrefs.SetString(ITSH_IDENTIFICATION, json);
            }
        }

        public static Itshe AddedLampsDefaultColor
        {
            get
            {
                var itshe = new Itshe { E = 1.0f, I = 0.5f };

                if (!PlayerPrefs.HasKey(ITSH_DEFAULT)) return itshe;
                
                var json = PlayerPrefs.GetString(ITSH_DEFAULT);
                itshe = JsonConvert.DeserializeObject<Itshe>(json);

                return itshe;
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
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

        public static string IOSBluetoothWifiSsid
        {
            get => GetString(SETT_IOS_BLE_WIFI_SSID, "VoyagerRouter");
            set => SetString(SETT_IOS_BLE_WIFI_SSID, value);
        }

        public static string SetClientSsid
        {
            get => GetString(SET_CLIENT_SSID, "Voyager Router");
            set => SetString(SET_CLIENT_SSID, value);
        }

        public static string SetClientPassword
        {
            get => GetString(SET_CLIENT_PASSWORD, "dsputnik");
            set => SetString(SET_CLIENT_PASSWORD, value);
        }

        public static bool AutoLoad
        {
            get => GetBool(AUTO_LOAD, true);
            set => SetBool(AUTO_LOAD, value);
        }

        public static bool AutoSave
        {
            get => GetBool(AUTO_SAVE, true);
            set => SetBool(AUTO_SAVE, value);
        }

        public static int AutoSaveDelay
        {
            get => GetInt(AUTO_SAVE_DELAY, 15);
            set => SetInt(AUTO_SAVE_DELAY, value);
        }

        private static void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }

        private static bool GetBool(string key, bool defaultValue)
        {
            if (!PlayerPrefs.HasKey(key))
                return defaultValue;
            return PlayerPrefs.GetInt(key) == 1;
        }

        private static void SetString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }

        private static string GetString(string key, string defaultValue)
        {
            return !PlayerPrefs.HasKey(key) ? defaultValue : PlayerPrefs.GetString(key);
        }
        private static void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }

        private static int GetInt(string key, int defaultValue)
        {
            return !PlayerPrefs.HasKey(key) ? defaultValue : PlayerPrefs.GetInt(key);
        }
    }
}
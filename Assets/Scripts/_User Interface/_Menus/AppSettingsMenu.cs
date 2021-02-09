using DigitalSputnik.Colors;
using UnityEngine;
using UnityEngine.UI;

namespace VoyagerController.UI
{
    public class AppSettingsMenu : Menu
    {
        [SerializeField] private ItshePicker _identifyItsh      = null;
        [SerializeField] private ItshePicker _startColorItsh    = null;
        [Header("Info Settings")]
        [SerializeField] private Toggle _batteryLevelToggle   = null;
        [SerializeField] private Toggle _chargingStatusToggle = null;
        [SerializeField] private Toggle _wifiModeToggle       = null;
        [SerializeField] private Toggle _lenghtToggle         = null;
        [SerializeField] private Toggle _dmxUniverseToggle    = null;
        [SerializeField] private Toggle _dmxChannelToggle     = null;
        [SerializeField] private Toggle _ipAddressToggle      = null;
        [SerializeField] private Toggle _firmwareVersion      = null;
        
        internal override void OnShow()
        {
            _identifyItsh.Value = ApplicationSettings.IdentificationColor;
            _startColorItsh.Value = ApplicationSettings.AddedLampsDefaultColor;

            _batteryLevelToggle.isOn = ApplicationSettings.ShowInfoBatteryLevel;
            _chargingStatusToggle.isOn = ApplicationSettings.ShowInfoChargingStatus;
            _wifiModeToggle.isOn = ApplicationSettings.ShowInfoWifiMode;
            _lenghtToggle.isOn = ApplicationSettings.ShowInfoLenght;
            _dmxUniverseToggle.isOn = ApplicationSettings.ShowInfoDmxUniverse;
            _dmxChannelToggle.isOn = ApplicationSettings.ShowInfoDmxChannel;
            _ipAddressToggle.isOn = ApplicationSettings.ShowInfoIpAddress;
            _firmwareVersion.isOn = ApplicationSettings.ShowInfoFirmwareVersion;
            
            
            _identifyItsh.OnValueChanged.AddListener(OnIdentifyColorPicked);
            _startColorItsh.OnValueChanged.AddListener(OnLampAddedToWorkspaceFirstTime);
            
            _batteryLevelToggle.onValueChanged.AddListener(OnBatteryLevelToggleChanged);
            _chargingStatusToggle.onValueChanged.AddListener(OnChargingStatusToggleChanged);
            _wifiModeToggle.onValueChanged.AddListener(OnWifiModeToggleChanged);
            _lenghtToggle.onValueChanged.AddListener(OnLenghtToggleChanged);
            _dmxUniverseToggle.onValueChanged.AddListener(OnDmxUniverseToggleChanged);
            _dmxChannelToggle.onValueChanged.AddListener(OnDmxChannelToggleChanged);
            _ipAddressToggle.onValueChanged.AddListener(OnIpAddressToggleChanged);
            _firmwareVersion.onValueChanged.AddListener(OnFirmwareVersionChanged);
        }

        internal override void OnHide()
        {
            _identifyItsh.OnValueChanged.RemoveListener(OnIdentifyColorPicked);
            _startColorItsh.OnValueChanged.RemoveListener(OnLampAddedToWorkspaceFirstTime);

            _batteryLevelToggle.onValueChanged.RemoveListener(OnBatteryLevelToggleChanged);
            _chargingStatusToggle.onValueChanged.RemoveListener(OnChargingStatusToggleChanged);
            _wifiModeToggle.onValueChanged.RemoveListener(OnWifiModeToggleChanged);
            _lenghtToggle.onValueChanged.RemoveListener(OnLenghtToggleChanged);
            _dmxUniverseToggle.onValueChanged.RemoveListener(OnDmxUniverseToggleChanged);
            _dmxChannelToggle.onValueChanged.RemoveListener(OnDmxChannelToggleChanged);
            _ipAddressToggle.onValueChanged.RemoveListener(OnIpAddressToggleChanged);
            _firmwareVersion.onValueChanged.RemoveListener(OnFirmwareVersionChanged);
        }

        private static void OnIdentifyColorPicked(Itshe itshe) => ApplicationSettings.IdentificationColor = itshe;
        private static void OnLampAddedToWorkspaceFirstTime(Itshe itshe) => ApplicationSettings.AddedLampsDefaultColor = itshe;
        private static void OnBatteryLevelToggleChanged(bool value) => ApplicationSettings.ShowInfoBatteryLevel = value;
        private static void OnChargingStatusToggleChanged(bool value) => ApplicationSettings.ShowInfoChargingStatus = value;
        private static void OnWifiModeToggleChanged(bool value) => ApplicationSettings.ShowInfoWifiMode = value;
        private static void OnLenghtToggleChanged(bool value) => ApplicationSettings.ShowInfoLenght = value;
        private static void OnDmxUniverseToggleChanged(bool value) => ApplicationSettings.ShowInfoDmxUniverse = value;
        private static void OnDmxChannelToggleChanged(bool value) => ApplicationSettings.ShowInfoDmxChannel = value;
        private static void OnIpAddressToggleChanged(bool value) => ApplicationSettings.ShowInfoIpAddress = value;
        private static void OnFirmwareVersionChanged(bool value) => ApplicationSettings.ShowInfoFirmwareVersion = value;
    }
}
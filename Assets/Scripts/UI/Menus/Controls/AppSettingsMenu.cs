using UnityEngine;
using UnityEngine.UI;

namespace VoyagerApp.UI.Menus
{
    public class AppSettingsMenu : Menu
    {
        [SerializeField] ItshPickView identifyItsh      = null;
        [SerializeField] ItshPickView startColorItsh    = null;
        [Header("Info Settings")]
        [SerializeField] Toggle batteryLevelToggle   = null;
        [SerializeField] Toggle chargingStatusToggle = null;
        [SerializeField] Toggle wifiModeToggle       = null;
        [SerializeField] Toggle lenghtToggle         = null;
        [SerializeField] Toggle dmxUniverseToggle    = null;
        [SerializeField] Toggle dmxChannelToggle     = null;
        [SerializeField] Toggle ipAddressToggle      = null;

        internal override void OnShow()
        {
            identifyItsh.Value = ApplicationSettings.IdentificationColor;
            identifyItsh.onValueChanged.AddListener(OnIdentifyColorPicked);

            startColorItsh.Value = ApplicationSettings.AddedLampsDefaultColor;
            startColorItsh.onValueChanged.AddListener(OnLampAddedToWorkspaceFirstTime);

            batteryLevelToggle.isOn = ApplicationSettings.ShowInfoBatteryLevel;
            batteryLevelToggle.onValueChanged.AddListener(OnBatteryLevelToggleChanged);

            chargingStatusToggle.isOn = ApplicationSettings.ShowInfoChargingStatus;
            chargingStatusToggle.onValueChanged.AddListener(OnChargingStatusToggleChanged);

            wifiModeToggle.isOn = ApplicationSettings.ShowInfoWifiMode;
            wifiModeToggle.onValueChanged.AddListener(OnWifiModeToggleChanged);

            lenghtToggle.isOn = ApplicationSettings.ShowInfoLenght;
            lenghtToggle.onValueChanged.AddListener(OnLenghtToggleChanged);

            dmxUniverseToggle.isOn = ApplicationSettings.ShowInfoDmxUniverse;
            dmxUniverseToggle.onValueChanged.AddListener(OnDmxUniverseToggleChanged);

            dmxChannelToggle.isOn = ApplicationSettings.ShowInfoDmxChannel;
            dmxChannelToggle.onValueChanged.AddListener(OnDmxChannelToggleChanged);

            ipAddressToggle.isOn = ApplicationSettings.ShowInfoIpAddress;
            ipAddressToggle.onValueChanged.AddListener(OnIpAddressToggleChanged);
        }

        internal override void OnHide()
        {
            identifyItsh.onValueChanged.RemoveListener(OnIdentifyColorPicked);
            startColorItsh.onValueChanged.RemoveListener(OnLampAddedToWorkspaceFirstTime);

            batteryLevelToggle.onValueChanged.RemoveListener(OnBatteryLevelToggleChanged);
            chargingStatusToggle.onValueChanged.RemoveListener(OnChargingStatusToggleChanged);
            wifiModeToggle.onValueChanged.RemoveListener(OnWifiModeToggleChanged);
            lenghtToggle.onValueChanged.RemoveListener(OnLenghtToggleChanged);
            dmxUniverseToggle.onValueChanged.RemoveListener(OnDmxUniverseToggleChanged);
            dmxChannelToggle.onValueChanged.RemoveListener(OnDmxChannelToggleChanged);
            ipAddressToggle.onValueChanged.RemoveListener(OnIpAddressToggleChanged);
        }

        void OnIdentifyColorPicked(Itshe itshe)
        {
            ApplicationSettings.IdentificationColor = itshe;
        }

        void OnLampAddedToWorkspaceFirstTime(Itshe itshe)
        {
            ApplicationSettings.AddedLampsDefaultColor = itshe;
        }

        void OnBatteryLevelToggleChanged(bool value)
        {
            ApplicationSettings.ShowInfoBatteryLevel = value;
        }

        void OnChargingStatusToggleChanged(bool value)
        {
            ApplicationSettings.ShowInfoChargingStatus = value;
        }

        void OnWifiModeToggleChanged(bool value)
        {
            ApplicationSettings.ShowInfoWifiMode = value;
        }

        void OnLenghtToggleChanged(bool value)
        {
            ApplicationSettings.ShowInfoLenght = value;
        }

        void OnDmxUniverseToggleChanged(bool value)
        {
            ApplicationSettings.ShowInfoDmxUniverse = value;
        }

        void OnDmxChannelToggleChanged(bool value)
        {
            ApplicationSettings.ShowInfoChargingStatus = value;
        }

        void OnIpAddressToggleChanged(bool value)
        {
            ApplicationSettings.ShowInfoIpAddress = value;
        }
    }
}
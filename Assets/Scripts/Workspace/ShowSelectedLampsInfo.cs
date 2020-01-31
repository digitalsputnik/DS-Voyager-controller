using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VoyagerApp.Lamps;
using VoyagerApp.Lamps.Voyager;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;

namespace VoyagerApp.UI
{
    public class ShowSelectedLampsInfo : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] Color pressedColor = Color.white;
        [SerializeField] Color releasedColor = Color.white;
        [SerializeField] Image image = null;

        Dictionary<Lamp, string> prevInfo = new Dictionary<Lamp, string>();

        void Start()
        {
            ApplicationState.ColorWheelActive.onChanged += ColorWheelActiveChanged;
            WorkspaceSelection.instance.onSelectionChanged += OnSelectionChanged;
            OnPointerUp(null);
            gameObject.SetActive(false);
        }

        void ColorWheelActiveChanged(bool value)
        {
            gameObject.SetActive(!value);
        }

        void OnDestroy()
        {
            ApplicationState.ColorWheelActive.onChanged -= ColorWheelActiveChanged;
            WorkspaceSelection.instance.onSelectionChanged -= OnSelectionChanged;
        }

        void OnSelectionChanged()
        {
            gameObject.SetActive(WorkspaceUtils.SelectedLamps.Count != 0);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            image.color = pressedColor;
            foreach (var item in WorkspaceUtils.SelectedVoyagerLampItems)
            {
                VoyagerLamp lamp = item.lamp;
                if (!prevInfo.ContainsKey(lamp))
                    prevInfo.Add(lamp, item.suffix);
                item.SetSuffix(InfoOfLamp(lamp));
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            image.color = releasedColor;
            foreach (var item in WorkspaceUtils.SelectedVoyagerLampItems)
            {
                VoyagerLamp lamp = item.lamp;
                string info = prevInfo[lamp];
                item.SetSuffix(info);
            }
            prevInfo.Clear();
        }

        string InfoOfLamp(VoyagerLamp lamp)
        {
            List<string> info = new List<string>();
            if (ApplicationSettings.ShowInfoLenght)
                info.Add(lamp.length > 50 ? "4ft" : "2ft");
            if (ApplicationSettings.ShowInfoBatteryLevel)
                info.Add($"{lamp.battery}%");
            if (ApplicationSettings.ShowInfoChargingStatus && lamp.charging)
                info.Add("charging");
            if (ApplicationSettings.ShowInfoWifiMode)
                info.Add(ModeFromString(lamp.mode));
            if (lamp.dmxEnabled)
            {
                if (ApplicationSettings.ShowInfoDmxUniverse)
                    info.Add($"universe {lamp.dmxUniverse}");
                if (ApplicationSettings.ShowInfoDmxChannel)
                    info.Add($"channel {lamp.dmxChannel + 1}");
            }
            if (ApplicationSettings.ShowInfoIpAddress)
                info.Add($"{lamp.address}");
            if (ApplicationSettings.ShowInfoFirmwareVersion)
                info.Add($"{lamp.version}");
            return string.Join(", ", info);
        }

        string ModeFromString(string mode)
        {
            switch(mode)
            {
                case "ap_mode":
                    return "MASTER";
                case "client_mode":
                    return "CLIENT";
                case "router_mode":
                    return "ROUTER";
            }
            return "";
        }
    }
}

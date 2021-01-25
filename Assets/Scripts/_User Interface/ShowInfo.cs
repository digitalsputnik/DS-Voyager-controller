using System.Collections.Generic;
using System.Linq;
using DigitalSputnik;
using DigitalSputnik.Voyager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VoyagerController.Workspace;

namespace VoyagerController.UI
{
    public class ShowInfo : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Color _pressedColor = Color.white;
        [SerializeField] private Color _releasedColor = Color.white;
        [SerializeField] private Image _image = null;

        private readonly Dictionary<Lamp, string> _prevInfo = new Dictionary<Lamp, string>();

        private void Start()
        {
            ApplicationState.ColorWheelActive.OnChanged += ColorWheelActiveChanged;
            WorkspaceSelection.SelectionChanged += OnSelectionChanged;
            OnPointerUp(null);
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            ApplicationState.ColorWheelActive.OnChanged -= ColorWheelActiveChanged;
            WorkspaceSelection.SelectionChanged -= OnSelectionChanged;
        }

        private void ColorWheelActiveChanged(bool value)
        {
            gameObject.SetActive(!value);
        }

        private void OnSelectionChanged()
        {
            gameObject.SetActive(WorkspaceSelection.GetSelected<VoyagerItem>().Any());
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _image.color = _pressedColor;
            foreach (var item in WorkspaceSelection.GetSelected<VoyagerItem>())
            {
                var lamp = item.LampHandle;
                if (!_prevInfo.ContainsKey(lamp))
                    _prevInfo.Add(lamp, item.Suffix);
                item.Suffix = InfoOfLamp(lamp);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _image.color = _releasedColor;
            foreach (var item in WorkspaceSelection.GetSelected<VoyagerItem>())
            {
                var lamp = item.LampHandle;
                var info = _prevInfo[lamp];
                item.Suffix = info;
            }
            _prevInfo.Clear();
        }

        private static string InfoOfLamp(VoyagerLamp lamp)
        {
            var info = new List<string>();
            
            if (ApplicationSettings.ShowInfoLenght)
                info.Add(lamp.PixelCount > 50 ? "4ft" : "2ft");
            
            // TODO: Finish!
            
            /*
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
            */

            if (lamp.Endpoint is LampNetworkEndPoint endpoint)
            {
                if (ApplicationSettings.ShowInfoIpAddress)
                    info.Add($"{endpoint.address}");   
            }

            if (ApplicationSettings.ShowInfoFirmwareVersion)
                info.Add($"{lamp.Version}");
            
            return string.Join(", ", info);
        }
    }
}
using DigitalSputnik;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VoyagerController.Bluetooth;
using VoyagerController.Workspace;

namespace VoyagerController.UI
{
    public class IdentifyLamps : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Color _pressedColor = Color.white;
        [SerializeField] private Color _releasedColor = Color.white;
        [SerializeField] private Image _image = null;
        [SerializeField] private float _networkDuration = 0.3f;
        [SerializeField] private float _bleDuration = 0.3f;

        private bool _send;
        private float _timestampBle;
        private float _timestampNet;

        private void Start()
        {
            WorkspaceSelection.SelectionChanged += OnSelectionChanged;
            OnPointerUp(null);
            gameObject.SetActive(false);
        }

        private void Update()
        {
            foreach (var lamp in WorkspaceSelection.GetSelected<VoyagerItem>())
            {
                if (_send && Time.time - _timestampBle > _bleDuration / 2.0f - 0.005)
                {
                    if (lamp.LampHandle.Endpoint is BluetoothEndPoint)
                        lamp.LampHandle.OverridePixels(ApplicationSettings.IdentificationColor, _bleDuration);

                    _timestampBle = Time.deltaTime;
                }

                if (_send && Time.time - _timestampNet > _networkDuration / 2.0f - 0.005)
                {
                    if (lamp.LampHandle.Endpoint is LampNetworkEndPoint)
                        lamp.LampHandle.OverridePixels(ApplicationSettings.IdentificationColor, _networkDuration);

                    _timestampNet = Time.deltaTime;
                }
            }
        }

        private void OnDestroy()
        {
            WorkspaceSelection.SelectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged()
        {
            gameObject.SetActive(WorkspaceSelection.GetSelected<VoyagerItem>().Any());
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _image.color = _pressedColor;
            _send = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _image.color = _releasedColor;
            _send = false;
        }
    }
}

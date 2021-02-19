using System;
using DigitalSputnik.Voyager;
using UnityEngine;
using UnityEngine.UI;
using VoyagerController.Bluetooth;

namespace VoyagerController.UI
{
    public class AddLampItem : MonoBehaviour
    {
        [SerializeField] private Image _bleIcon = null;

        public string Serial;
    
        private Text _text;
        private Action _action;

        public void Awake()
        {
            _text = GetComponentInChildren<Text>();
        }

        public void Setup(VoyagerLamp voyager, Action action)
        {
            Serial = voyager.Serial;
            _text.text = voyager.Serial;
            _action = action;
            _bleIcon.gameObject.SetActive(voyager.Endpoint is BluetoothEndPoint);
        }

        public void Click()
        {
            _action?.Invoke();
        }
    }
}

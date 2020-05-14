using UnityEngine;
using UnityEngine.UI;

namespace VoyagerApp.UI.Menus
{
    public class BluetoothLampItem : MonoBehaviour
    {
        [SerializeField] Toggle _toggle;
        [SerializeField] Text _nameText;

        public string BluetoothId { get; set; }

        public bool Toggled
        {
            get => _toggle.isOn;
            set => _toggle.isOn = value;
        }

        public string Name
        {
            get => _nameText.text;
            set => _nameText.text = value;
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}
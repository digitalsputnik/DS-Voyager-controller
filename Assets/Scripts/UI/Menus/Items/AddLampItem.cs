using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Lamps;
using VoyagerApp.Utilities;

namespace VoyagerApp.UI.Menus
{
    public class AddLampItem : MonoBehaviour
    {
        [SerializeField] Text serialText = null;

        public Lamp lamp;

        public void SetLamp(Lamp lamp)
        {
            this.lamp = lamp;
            serialText.text = lamp.serial;
        }

        public void OnClick()
        {
            Vector2 position = VectorUtils.ScreenRandomVerticalPosition;
            lamp.AddToWorkspace(position);
            AddLampsMenu menu = GetComponentInParent<AddLampsMenu>();
            menu.RemoveLampItem(this);
        }
    }
}
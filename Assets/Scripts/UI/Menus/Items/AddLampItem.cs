using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Lamps;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;

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
            var vlamp = lamp.AddToWorkspace();
            WorkspaceSelection.instance.Clear();
            WorkspaceSelection.instance.SelectItem(vlamp);
            //AddLampsMenu menu = GetComponentInParent<AddLampsMenu>();
            //menu.RemoveLampItem(this);
        }
    }
}
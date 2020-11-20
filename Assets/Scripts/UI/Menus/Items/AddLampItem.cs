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
            WorkspaceSelection.instance.Clear();
            var vlamp = lamp.AddToWorkspace(WorkspaceUtils.PositionOfLastNotSelectedLamp + new Vector3(0,-1.0f,0));
            WorkspaceUtils.SetCameraPosition(vlamp.transform.localPosition);
            WorkspaceSelection.instance.SelectItem(vlamp);
            //AddLampsMenu menu = GetComponentInParent<AddLampsMenu>();
            //menu.RemoveLampItem(this);
        }
    }
}
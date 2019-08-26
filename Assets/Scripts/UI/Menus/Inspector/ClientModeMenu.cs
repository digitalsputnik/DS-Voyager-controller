using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Utilities;

namespace VoyagerApp.UI.Menus
{
    public class ClientModeMenu : Menu
    {
        [SerializeField] Text infoText              = null;
        [SerializeField] Toggle ssidsToggle         = null;
        [SerializeField] Dropdown ssidsDropdown     = null;
        [SerializeField] Toggle ssidToggle          = null;
        [SerializeField] InputField ssidField       = null;
        [SerializeField] InputField passwordField   = null;
        [SerializeField] Button setButton           = null;

        public void Set()
        {
            var client = NetUtils.VoyagerClient;
            var ssid = ssidField.text;
            var password = passwordField.text;

            foreach (var lamp in WorkspaceUtils.SelectedLamps)
                client.TurnToClient(lamp, ssid, password);

            GetComponentInParent<InspectorMenuContainer>().ShowMenu(null);
        }
    }
}
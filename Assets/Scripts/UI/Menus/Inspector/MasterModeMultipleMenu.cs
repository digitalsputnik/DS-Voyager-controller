using VoyagerApp.Utilities;

namespace VoyagerApp.UI.Menus
{
    public class MasterModeMultipleMenu : Menu
    {
        public void Set()
        {
            var client = NetUtils.VoyagerClient;
            foreach (var lamp in WorkspaceUtils.Lamps)
                client.TurnToMaster(lamp);
            GetComponentInParent<InspectorMenuContainer>().ShowMenu(null);
        }
    }
}
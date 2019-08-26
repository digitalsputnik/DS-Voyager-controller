using VoyagerApp.Utilities;

namespace VoyagerApp.UI.Menus
{
    public class RouterModeMenu : Menu
    {
        public void Set()
        {
            var client = NetUtils.VoyagerClient;
            foreach (var lamp in WorkspaceUtils.Lamps)
                client.TurnToRouter(lamp);
            GetComponentInParent<InspectorMenuContainer>().ShowMenu(null);
        }
    }
}
using System.Linq;
using DigitalSputnik.Voyager;
using VoyagerController.Workspace;

namespace VoyagerController.UI
{
    public class RouterModeMenu : Menu
    {
        public void Set()
        {
            foreach (var voyager in WorkspaceSelection.GetSelected<VoyagerItem>().Select(i => i.LampHandle))
                voyager.SetNetworkMode(NetworkMode.Router);
            GetComponentInParent<InspectorMenuContainer>().ShowMenu(null);
        }
    }
}
using System.Linq;
using DigitalSputnik.Voyager;
using VoyagerController.Workspace;

namespace VoyagerController.UI
{
    public class MasterModeMenu : Menu
    {
        public void Set()
        {
            foreach (var voyager in WorkspaceSelection.GetSelected<VoyagerItem>().Select(i => i.LampHandle))
                voyager.SetNetworkMode(NetworkMode.Master);
            GetComponentInParent<InspectorMenuContainer>().ShowMenu(null);
        }
    }
}
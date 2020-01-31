using VoyagerApp.Lamps;
using VoyagerApp.UI.Overlays;
using VoyagerApp.Videos;
using VoyagerApp.Workspace;

namespace VoyagerApp.UI.Menus
{
    public class SetupMenu : Menu
    {
        public void NewProject()
        {
            DialogBox.Show(
                "NEW PROJECT",
                "All unsaved project changes will be discarded",
                "CANCEL", "OK", null,
                () =>
                {
                    WorkspaceSelection.instance.Clear();
                    WorkspaceManager.instance.Clear();
                    LampManager.instance.Clear();
                    VideoManager.instance.Clear();
                    VideoManager.instance.LoadPresets();
                });
        }
    }
}
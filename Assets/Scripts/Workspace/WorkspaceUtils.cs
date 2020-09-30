using System.Collections.Generic;
using System.Linq;

namespace VoyagerController.Workspace
{
    public static class WorkspaceUtils
    {
        public static IEnumerable<WorkspaceItem> SelectableItems => WorkspaceManager.GetItems().Where(i => i.Selectable);
        
        public static List<VoyagerItem> SelectedVoyagerItemsInOrder
        {
            get => WorkspaceManager.GetItems<VoyagerItem>().OrderBy(l => l.Order).ToList();
        }
    }
}
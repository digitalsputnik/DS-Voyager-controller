using System.Collections.Generic;
using System.Linq;
using DigitalSputnik.Voyager;
using VoyagerController.Effects;

namespace VoyagerController.Workspace
{
    public static class WorkspaceUtils
    {
        public static IEnumerable<WorkspaceItem> SelectableItems =>
            WorkspaceManager.GetItems().Where(i => i.Selectable);
        
        public static List<VoyagerItem> SelectedVoyagerItemsInOrder =>
            WorkspaceManager.GetItems<VoyagerItem>().OrderBy(l => l.Order).ToList();

        public static bool AllLampsSelected => WorkspaceSelection.GetSelected<VoyagerItem>().Count() ==
                                               WorkspaceManager.GetItems<VoyagerItem>().Count();

        public static void SelectAllLamps()
        {
            foreach (var lamp in WorkspaceManager.GetItems<VoyagerItem>())
                WorkspaceSelection.SelectItem(lamp);
        }

        public static void DeselectAllLamps()
        {
            foreach (var lamp in WorkspaceManager.GetItems<VoyagerItem>())
                WorkspaceSelection.DeselectItem(lamp);
        }

        public static IEnumerable<VoyagerItem> GetItemsWithSameEffect(Effect effect)
        {
            return WorkspaceManager
                .GetItems<VoyagerItem>()
                .Where(v => ApplicationManager.Lamps.GetMetadata(v.LampHandle.Serial).Effect == effect);
        }
    }
}
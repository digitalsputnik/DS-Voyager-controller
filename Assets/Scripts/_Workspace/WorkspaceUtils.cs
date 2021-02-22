using System.Collections.Generic;
using System.Linq;
using DigitalSputnik.Voyager;
using UnityEngine;
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
            foreach (var lamp in WorkspaceManager.GetItems<VoyagerItem>().ToArray())
                WorkspaceSelection.SelectItem(lamp);
        }

        public static void DeselectAllLamps()
        {
            foreach (var lamp in WorkspaceManager.GetItems<VoyagerItem>().ToArray())
                WorkspaceSelection.DeselectItem(lamp);
        }

        public static IEnumerable<VoyagerItem> GetItemsWithSameEffect(Effect effect)
        {
            return WorkspaceManager
                .GetItems<VoyagerItem>()
                .Where(v => Metadata.GetLamp(v.LampHandle.Serial).Effect == effect);
        }

        public static Vector3 PositionOfLastSelectedOrAddedLamp
        {
            get
            {
                var pos = Vector3.zero;

                if (WorkspaceSelection.LastSelectedLamp != null)
                {
                    var lastSelected = WorkspaceSelection.LastSelectedLamp;
                    pos = lastSelected.transform.localPosition;

                    if (WorkspaceSelection.Contains(lastSelected))
                        pos = pos + lastSelected.transform.parent.parent.localPosition;
                    
                }
                else if (WorkspaceManager.GetItems<VoyagerItem>().ToList().Count() > 0)
                {
                    var lastAdded = WorkspaceManager.GetItems<VoyagerItem>().ToList().Last();
                    pos = lastAdded.transform.localPosition;

                    if (WorkspaceSelection.Contains(lastAdded))
                        pos = pos + lastAdded.transform.parent.parent.localPosition;
                }

                return pos;
            }
        }
    }
}
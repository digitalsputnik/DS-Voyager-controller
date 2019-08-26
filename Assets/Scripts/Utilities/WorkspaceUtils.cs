using System.Collections.Generic;
using System.Linq;
using VoyagerApp.Lamps;
using VoyagerApp.Lamps.Voyager;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.Utilities
{
    public static class WorkspaceUtils
    {
        public static List<Lamp> SelectedLamps
        {
            get
            {
                List<Lamp> lamps = new List<Lamp>();
                WorkspaceSelection.instance
                                  .Selected
                                  .ForEach(view => lamps.Add(view.lamp));
                return lamps;
            }
        }

        public static List<VoyagerLamp> SelectedVoyagerLamps
        {
            get
            {
                List<VoyagerLamp> lamps = new List<VoyagerLamp>();
                SelectedLamps.ForEach((lamp) =>
                {
                    if (lamp is VoyagerLamp vLamp)
                        lamps.Add(vLamp);
                });
                return lamps;
            }
        }

        public static List<Lamp> Lamps
        {
            get
            {
                List<Lamp> lamps = new List<Lamp>();
                WorkspaceManager.instance
                                .GetItemsOfType<LampItemView>().ToList()
                                .ForEach(view => lamps.Add(view.lamp));
                return lamps;
            }
        }

        public static List<VoyagerLamp> VoyagerLamps
        {
            get
            {
                List<VoyagerLamp> lamps = new List<VoyagerLamp>();
                WorkspaceManager.instance
                                .GetItemsOfType<VoyagerItemView>().ToList()
                                .ForEach(view => lamps.Add(view.lamp));
                return lamps;
            }
        }

        public static List<LampItemView> SelectedLampItems
        {
            get => WorkspaceSelection.instance.Selected;
        }

        public static List<VoyagerItemView> SelectedVoyagerLampItems
        {
            get
            {
                List<VoyagerItemView> lamps = new List<VoyagerItemView>();
                SelectedLampItems.ForEach((item) =>
                {
                    if (item is VoyagerItemView vItem)
                        lamps.Add(vItem);
                });
                return lamps;
            }
        }

        public static List<LampItemView> LampItems
        {
            get
            {
                return WorkspaceManager.instance
                                       .GetItemsOfType<LampItemView>()
                                       .ToList();
            }
        }

        public static List<VoyagerItemView> VoyagerItems
        {
            get
            {
                return WorkspaceManager.instance
                                       .GetItemsOfType<VoyagerItemView>()
                                       .ToList();
            }
        }
    }
}
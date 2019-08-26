using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VoyagerApp.Utilities;
using VoyagerApp.Videos;

namespace VoyagerApp.UI.Menus
{
    public class LoadMenu : Menu
    {
        [SerializeField] LoadMenuItem itemPrefab    = null;
        [SerializeField] Transform container        = null;
        [SerializeField] VideoMapper mapper         = null;
        [SerializeField] bool wm                    = false;

        List<LoadMenuItem> items = new List<LoadMenuItem>();

        internal override void OnShow()
        {
            foreach (var item in new List<LoadMenuItem>(items))
            {
                items.Remove(item);
                Destroy(item.gameObject);
            }

            string[] saves = null;
            if (wm)
            {
                if (!Directory.Exists(FileUtils.WorkspaceSavesPath + "/vm"))
                    Directory.CreateDirectory(FileUtils.WorkspaceSavesPath + "/vm");
                saves = Directory.GetFiles(FileUtils.WorkspaceSavesPath + "/vm", "*.ws");
            }
            else
                saves = Directory.GetFiles(FileUtils.WorkspaceSavesPath, "*.ws");

            foreach (var save in saves)
            {
                LoadMenuItem item = Instantiate(itemPrefab, container);
                item.SetPath(save, wm);
                items.Add(item);
            }
        }

        internal void VideoMappingLoaded()
        {
            var lamps = WorkspaceUtils.LampItems;
            if (lamps.Count > 0) mapper.SetVideo(lamps[0].lamp.video);
        }
    }
}
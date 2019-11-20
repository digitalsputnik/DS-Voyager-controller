using System.Collections;
using System.Linq;
using UnityEngine;
using VoyagerApp.Lamps;
using VoyagerApp.Lamps.Voyager;
using VoyagerApp.Videos;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.UI
{
    public class CreateWhiteIfAddedToWorkspace : MonoBehaviour
    {
        void Start()
        {
            WorkspaceManager.instance.onItemAdded += OnLampAddedToWorkspace;
        }

        void OnDestroy()
        {
            WorkspaceManager.instance.onItemAdded -= OnLampAddedToWorkspace;
        }

        void OnLampAddedToWorkspace(WorkspaceItemView item)
        {
            if (item is LampItemView lampView)
                StartCoroutine(AddLamp(lampView.lamp));
        }

        IEnumerator AddLamp(Lamp lamp)
        {
            yield return new WaitForSeconds(0.3f);
            if (lamp.video == null && !((VoyagerLamp)lamp).dmxEnabled)
            {
                lamp.SetVideo(VideoManager.instance.Videos.FirstOrDefault(v => v.name == "white"));
                lamp.SetItshe(ApplicationSettings.AddedLampsDefaultColor);
            }
        }
    }
}

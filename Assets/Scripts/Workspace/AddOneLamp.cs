using System.Collections;
using System.Linq;
using UnityEngine;
using VoyagerApp.Lamps;
using VoyagerApp.Utilities;

namespace VoyagerApp.Workspace
{
    public class AddOneLamp : MonoBehaviour
    {
        void Awake()
        {
            LampManager.instance.onLampAdded += LampManager_LampAdded;
        }

        void LampManager_LampAdded(Lamp lamp)
        {
            StartCoroutine(WaitBeforeAdding(lamp));
            LampManager.instance.onLampAdded -= LampManager_LampAdded;
        }

        IEnumerator WaitBeforeAdding(Lamp lamp)
        {
            yield return new WaitForSeconds(0.2f);
            WorkspaceManager manager = WorkspaceManager.instance;
            Lamp[] lamps = WorkspaceUtils.Lamps.ToArray();

            if (!lamps.Any(l => l == lamp))
                lamp.AddToWorkspace();

            enabled = false;
        }
    }
}
using System.Linq;
using UnityEngine;
using VoyagerApp.Lamps;
using VoyagerApp.Utilities;

namespace VoyagerApp.Workspace
{
    public class AddOneLamp : MonoBehaviour
    {
        [SerializeField] float seconds = 2.0f;

        void Awake()
        {
            LampManager.instance.onLampAdded += LampManager_LampAdded;
        }

        void LampManager_LampAdded(Lamp lamp)
        {
            WorkspaceManager manager = WorkspaceManager.instance;
            Lamp[] lamps = WorkspaceUtils.Lamps.ToArray();

            if (!lamps.Any(l => l == lamp))
            {
                lamp.AddToWorkspace();
                LampManager.instance.onLampAdded -= LampManager_LampAdded;
            }

            enabled = false;
        }
    }
}
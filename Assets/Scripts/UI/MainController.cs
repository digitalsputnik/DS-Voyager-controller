using System.IO;
using UnityEngine;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;

namespace VoyagerApp.UI
{
    public class MainController : MonoBehaviour
    {
        void Start()
        {
            CheckIfWorkspaceExists();
        }

        void CheckIfWorkspaceExists()
        {
            if (File.Exists(FileUtils.WorkspaceStatePathFull))
            {
                WorkspaceSaveLoad.Load(FileUtils.WorkspaceStatePath);
                File.Delete(FileUtils.WorkspaceStatePathFull);
            }
        }
    }
}
using System.IO;
using UnityEngine;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;

namespace VoyagerApp.UI
{
    public class VideoMappingController : MonoBehaviour
    {
        void Start()
        {
            CheckIfSelectionFileExistsAndLoad();
        }

        void CheckIfSelectionFileExistsAndLoad()
        {
            if (File.Exists(FileUtils.SelectionFilePathFull))
            {
                WorkspaceSaveLoad.Load(FileUtils.SelectionFilePath);
                File.Delete(FileUtils.SelectionFilePathFull);
            }
        }
    }
}

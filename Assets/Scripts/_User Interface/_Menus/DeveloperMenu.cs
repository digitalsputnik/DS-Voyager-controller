using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using VoyagerController.Workspace;

namespace VoyagerController.UI
{
    public class DeveloperMenu : Menu
    {
        [SerializeField] private Text _updateStateText = null;

        public void UploadUpdate()
        {
            FileUtils.PickFile(OnUpdatePicked);
        }

        void OnUpdatePicked(string path)
        {
            if (path != null)
            {
                try
                {
                    var lamps = WorkspaceSelection.GetSelected<VoyagerItem>();
                    var update = File.ReadAllBytes(path);
                    foreach (var lamp in lamps)
                        VoyagerUpdater.UpdateLamp(lamp.LampHandle, update, OnUpdateFinished, OnUpdateMessage);
                }
                catch (Exception ex)
                {
                    DialogBox.Show(
                        "ERROR UPLOADING UPDATE",
                        ex.Message,
                        new string[] { "OK" },
                        new Action[] { null }
                    );
                }
            }
        }

        private void OnUpdateMessage(VoyagerUpdateMessage message)
        {
            MainThread.Dispatch(() => { _updateStateText.text = message.Lamp.Serial + " : " + message.Message; });
        }

        private void OnUpdateFinished(VoyagerUpdateResponse response)
        {
            MainThread.Dispatch(() => { _updateStateText.text = $"Lamp {response.Lamp.Serial} finished update process. {response.Error}"; });
        }
    }
}
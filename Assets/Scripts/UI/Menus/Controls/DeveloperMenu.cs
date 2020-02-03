using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Lamps.Voyager;
using VoyagerApp.UI.Overlays;
using VoyagerApp.Utilities;

namespace VoyagerApp.UI.Menus
{
    public class DeveloperMenu : Menu
    {
        [SerializeField] Text updateStateText = null;

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
                    var lamps = WorkspaceUtils.SelectedVoyagerLamps;
                    var update = File.ReadAllBytes(path);
                    VoyagerUpdateUtility utility = new VoyagerUpdateUtility(update);
                    lamps.ForEach(lamp => utility.UpdateLamp(lamp,
                                                             OnUpdateFinished,
                                                             OnUpdateMessage));
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

        void OnUpdateMessage(VoyagerUpdateMessage message)
        {
            MainThread.Dispach(() =>
            {
                updateStateText.text = message.lamp.serial + " : " + message.message;
            });
        }

        void OnUpdateFinished(VoyagerUpdateResponse response)
        {
            MainThread.Dispach(() =>
            {
                DialogBox.Show(
                    response.success ? "SUCCESS" : "FAILED",
                    $"Lamp {response.lamp.serial} finished update process. {response.error}",
                    new string[] { "OK" },
                    new Action[] { null });
            });
        }
    }
}
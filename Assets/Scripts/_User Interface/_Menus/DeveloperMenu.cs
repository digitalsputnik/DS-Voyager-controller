using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using VoyagerController.ProjectManagement;
using VoyagerController.Workspace;

namespace VoyagerController.UI
{
    public class DeveloperMenu : Menu
    {
        [SerializeField] private Text _updateStateText = null;
        [SerializeField] private IntField _autoSaveDelay = null;

        internal override void OnShow()
        {
            StartCoroutine(SetupDelayField());
        }

        internal override void OnHide()
        {
            _autoSaveDelay.OnChanged -= OnAutoSaveDelayChanged;
        }

        private IEnumerator SetupDelayField()
        {
            _autoSaveDelay.SetValue(ApplicationSettings.AutoSaveDelay);
            yield return new WaitForFixedUpdate();
            _autoSaveDelay.OnChanged += OnAutoSaveDelayChanged;
        }

        private void OnAutoSaveDelayChanged(int value) => ApplicationSettings.AutoSaveDelay = value;

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
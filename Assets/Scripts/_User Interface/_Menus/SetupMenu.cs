using DigitalSputnik;
using DigitalSputnik.Voyager;
using System;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using VoyagerController.ProjectManagement;
using VoyagerController.Workspace;

namespace VoyagerController.UI
{
    public class SetupMenu : Menu
    {
        [SerializeField] private InspectorMenuContainer _inspectorMenuContainer = null;
        [SerializeField] private GameObject _developmentBtn = null;
        [SerializeField] private Menu _tutorial = null;
        [SerializeField] private Text _updateText = null;

        internal override void OnShow()
        {
            _developmentBtn.SetActive(ApplicationState.DeveloperMode);
        }

        public void NewProject()
        {
            DialogBox.Show(
                "NEW PROJECT",
                "All unsaved project changes will be discarded",
                new string[] { "CANCEL", "OK" },
                new Action[]
                {
                    null,
                    () =>
                    {
                        Project.New();
                        ApplicationState.RaiseNewProject();
                    }
                });
        }

        public void OpenHelp()
        {
            DialogBox.Show(
                "Help",
                "Would you like to go through the tutorial on connecting lamps or go to the support page?",
                new string[] { "TUTORIAL", "SUPPORT PAGE", "CANCEL" },
                new Action[] {
                    () =>
                    {
                        if (WorkspaceManager.GetItems<VoyagerItem>().Count() > 0)
                        {
                            DialogBox.Show(
                                "ATTENTION",
                                "Starting the tutorial will clear out the workspace, would you like to continue?",
                                new[] {"CANCEL", "YES"},
                                new Action[]
                                {
                                    null,
                                    () =>
                                    {
                                        DialogBox.Paused = true;
                                        _inspectorMenuContainer.ShowMenu(_tutorial);
                                    }
                                });
                        }
                        else
                        {
                            DialogBox.Paused = true;
                            _inspectorMenuContainer.ShowMenu(_tutorial);
                        }
                    }
                    ,
                    () => { Application.OpenURL(ApplicationSettings.HELP_URL); }
                    ,
                    () => {}
                }
            );
        }

        public void ForceSelectUpdate()
        {
            try
            {
                var lamp = new VoyagerLamp();
                lamp.Serial = "Master Lamp";
                var endpoint = new LampNetworkEndPoint(IPAddress.Parse("172.20.0.1"));
                lamp.Endpoint = endpoint;
                VoyagerUpdater.UpdateLamp(lamp, OnUpdateFinished, OnUpdateMessage);
                _updateText.gameObject.SetActive(true);
            }
            catch (Exception ex)
            {
                DialogBox.Show(
                    "ERROR UPLOADING UPDATE",
                    ex.Message,
                    new string[] { "OK" },
                    new Action[] { null }
                );

                MainThread.Dispatch(() =>
                {
                    if (_updateText != null)
                        _updateText.text = "Lamp update failed";
                });
            }
        }

        private void OnUpdateFinished(VoyagerUpdateResponse response)
        {
            MainThread.Dispatch(() =>
            {
                if (_updateText != null)
                    _updateText.text = "Lamp successfully updated";
            });
        }

        private void OnUpdateMessage(VoyagerUpdateMessage message)
        {
            MainThread.Dispatch(() =>
            {
                if (_updateText != null)
                    _updateText.text = $"{message.Lamp.Serial}\n" +
                                       $"{message.Message.ToUpper()}";
            });
        }
    }
}
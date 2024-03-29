﻿using System;
using System.Linq;
using UnityEngine;
using VoyagerController.ProjectManagement;
using VoyagerController.Workspace;

namespace VoyagerController.UI
{
    public class SetupMenu : Menu
    {
        [SerializeField] private InspectorMenuContainer _inspectorMenuContainer = null;
        [SerializeField] private GameObject _developmentBtn = null;
        [SerializeField] private Menu _tutorial = null;

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
    }
}
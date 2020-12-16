using System;
using UnityEngine;
using VoyagerApp.Effects;
using VoyagerApp.Lamps;
using VoyagerApp.UI.Overlays;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.UI.Menus
{
    public class SetupMenu : Menu
    {
        [SerializeField] InspectorMenuContainer inspectorMenuContainer = null;
        [SerializeField] GameObject developmentBtn = null;
        [SerializeField] Menu Tutorial = null;

        internal override void OnShow()
        {
            developmentBtn.SetActive(ApplicationState.DeveloperMode);
        }

        public void NewProject()
        {
            DialogBox.Show(
                "NEW PROJECT",
                "All unsaved project changes will be discarded",
                new string[] { "CANCEL", "OK" },
                new Action[] { null,
                    () =>
                    {
                        WorkspaceSelection.instance.Clear();
                        WorkspaceManager.instance.Clear();
                        LampManager.instance.Clear();
                        EffectManager.Clear();
                        ApplicationState.RaiseNewProject();
                    }
                }
            );
        }

        public void OpenHelp()
        {
            DialogBox.Show(
                "Help",
                "Would you like to go through the tutorial on connecting lamps or go to the support page?",
                new string[] { "TUTORIAL", "SUPPORT PAGE", "CANCEL"},
                new Action[] {
                    () =>
                    {
                        if (WorkspaceManager.instance.GetItemsOfType<VoyagerItemView>().Length > 0)
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
                                        DialogBox.PauseDialogues();
                                        inspectorMenuContainer.ShowMenu(Tutorial);
                                    }
                                });
                        }
                        else
                        {
                            DialogBox.PauseDialogues();
                            inspectorMenuContainer.ShowMenu(Tutorial); 
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
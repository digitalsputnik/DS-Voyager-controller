using System;
using UnityEngine;
using VoyagerApp.Effects;
using VoyagerApp.Lamps;
using VoyagerApp.UI.Overlays;
using VoyagerApp.Workspace;

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
                "Would you like to do the tutorial again or get to the support page?",
                new string[] { "TUTORIAL", "SUPPORT PAGE", "EXIT"},
                new Action[] {
                    () =>
                    {
                        DialogBox.PauseDialogues();
                        inspectorMenuContainer.ShowMenu(Tutorial);
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
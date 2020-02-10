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
        [SerializeField] GameObject developmentBtn = null;

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
            Application.OpenURL(ApplicationSettings.HELP_URL);
        }
    }
}
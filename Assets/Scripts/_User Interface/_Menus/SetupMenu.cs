using UnityEngine;

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
            // TODO: Implement!
        }

        public void OpenHelp()
        { 
            // TODO: Implement!
            /*
            DialogBox.Show(
                "Help",
                "Would you like to do the tutorial again or get to the support page?",
                new string[] { "TUTORIAL", "SUPPORT PAGE", "EXIT"},
                new Action[] {
                    () =>
                    {
                        DialogBox.PauseDialogues();
                        _inspectorMenuContainer.ShowMenu(_tutorial);
                    }
                    ,
                    () => { Application.OpenURL(ApplicationSettings.HELP_URL); }
                    ,
                    () => {}
                }
            );
            */
        }
    }
}
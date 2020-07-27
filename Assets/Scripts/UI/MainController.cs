using UnityEngine;
using VoyagerApp.UI.Overlays;

namespace VoyagerApp.UI
{
    public class MainController : MonoBehaviour
    {
        [SerializeField] InspectorMenuContainer inspectorMenuContainer = null;
        [SerializeField] Menu addAllLampsMenu = null;
        [SerializeField] Menu Tutorial = null;

        void Start()
        {
            CheckIfWorkspaceExists();
        }

        void CheckIfWorkspaceExists()
        {
            if (PlayerPrefs.HasKey("from_video_mapping"))
            {
                Projects.Project.LoadWorkspace();
                PlayerPrefs.DeleteKey("from_video_mapping");
            }
            else if (!PlayerPrefs.HasKey("TutorialDone"))
            {
                DialogBox.PauseDialogues();
                inspectorMenuContainer.ShowMenu(Tutorial);
            }
            else
            {
                inspectorMenuContainer.ShowMenu(addAllLampsMenu);
                ApplicationState.GlobalDimmer.value = 1.0f;
            }
        }
    }
}
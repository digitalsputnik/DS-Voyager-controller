using UnityEngine;
using VoyagerApp.UI.Menus;
using VoyagerApp.Workspace;

namespace VoyagerApp.UI
{
    public class TutorialLampsAdded : Tutorial
    {
        public override void CheckForAction()
        {
            if (WorkspaceManager.instance.Items.Count > 0)
            {
                TutorialManager.Instance.NextTutorial();
            }
        }
    }
}

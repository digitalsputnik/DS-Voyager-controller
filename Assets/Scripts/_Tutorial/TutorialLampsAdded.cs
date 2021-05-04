using System.Linq;
using VoyagerController.Workspace;

namespace VoyagerController.UI
{
    public class TutorialLampsAdded : Tutorial
    {
        public override void CheckForAction()
        {
            if (WorkspaceManager.GetItems<VoyagerItem>().Count() > 0)
            {
                TutorialManager.Instance.NextTutorial();
            }
        }
    }
}

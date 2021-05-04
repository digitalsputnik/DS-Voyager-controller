using UnityEngine;

namespace VoyagerController.UI
{
    public class TutorialClientPicked : Tutorial
    {
        [SerializeField] int nextTutorial;

        public override void CheckForAction()
        {
            TutorialManager.setClientPicked = true;
            TutorialManager.Instance.SetNextTutorial(nextTutorial);
        }
    }
}

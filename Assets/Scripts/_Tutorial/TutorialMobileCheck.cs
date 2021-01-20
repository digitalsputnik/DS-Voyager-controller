using UnityEngine;

namespace VoyagerController.UI
{
    public class TutorialMobileCheck : Tutorial
    {
        public int IfMobile;
        public int IfDesktop;

        public override void CheckForAction()
        {
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                TutorialManager.Instance.SetNextTutorial(IfMobile);
            }
            else
            {
                TutorialManager.Instance.SetNextTutorial(IfDesktop);
            }
        }
    }
}

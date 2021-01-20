using UnityEngine;
using UnityEngine.UI;

namespace VoyagerController.UI
{
    public class TutorialCustomButton : Tutorial
    {
        [SerializeField] private Button Button = null;

        public override void CheckForAction()
        {
            if (!TutorialManager.Instance.setup)
            {
                Button.onClick.RemoveListener(OnClick);
                Button.onClick.AddListener(OnClick);
                TutorialManager.Instance.setup = true;
            }
        }
        public void OnClick()
        {
            Button.onClick.RemoveListener(OnClick);
            TutorialManager.Instance.NextTutorial();
        }
    }
}

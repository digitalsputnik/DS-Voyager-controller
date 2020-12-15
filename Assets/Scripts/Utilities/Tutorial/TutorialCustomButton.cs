using UnityEngine;
using VoyagerApp.UI.Menus;
using UnityEngine.UI;

namespace VoyagerApp.UI
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

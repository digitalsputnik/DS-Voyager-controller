using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VoyagerController.UI
{
    public class TutorialCustomButton : Tutorial
    {
        [SerializeField] private Button Button = null;
        [SerializeField] public List<Button> buttonsToDisable = new List<Button>();

        public override void CheckForAction()
        {
            if (!TutorialManager.Instance.setup)
            {
                Button.onClick.RemoveListener(OnClick);
                Button.onClick.AddListener(OnClick);

                foreach (var button in buttonsToDisable)
                    button.interactable = false;

                TutorialManager.Instance.setup = true;
            }
        }
        public void OnClick()
        {
            Button.onClick.RemoveListener(OnClick);

            foreach (var button in buttonsToDisable)
                button.interactable = true;

            TutorialManager.Instance.NextTutorial();
        }
    }
}

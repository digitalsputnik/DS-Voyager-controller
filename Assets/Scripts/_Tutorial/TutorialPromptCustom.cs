using UnityEngine;
using UnityEngine.UI;

namespace VoyagerController.UI
{
    public class TutorialPromptCustom : Tutorial
    {
        [SerializeField] string leftButtonText = null;
        [SerializeField] string middleButtonText = null;
        [SerializeField] string rightButtonText = null;

        [SerializeField] int ifLeft;
        [SerializeField] int ifMiddle;
        [SerializeField] int ifRight;

        [SerializeField] public TutorialManager.TutorialChoice ifLeftChoice = TutorialManager.TutorialChoice.None;
        [SerializeField] public TutorialManager.TutorialChoice ifMiddleChoice = TutorialManager.TutorialChoice.None;
        [SerializeField] public TutorialManager.TutorialChoice ifRightChoice = TutorialManager.TutorialChoice.None;

        private Text leftButtonTextObject = null;
        private Text middleButtonTextObject = null;
        private Text rightButtonTextObject = null;

        public override void CheckForAction()
        {
            if (!TutorialManager.Instance.setup)
            {
                Setup();
                TutorialManager.Instance.setup = true;
            }
        }

        public void Setup()
        {
            TutorialManager.Instance.leftButton.onClick.RemoveAllListeners();
            TutorialManager.Instance.middleButton.onClick.RemoveAllListeners();
            TutorialManager.Instance.rightButton.onClick.RemoveAllListeners();
            TutorialManager.Instance.leftButton.onClick.AddListener(OnLeftClick);
            TutorialManager.Instance.middleButton.onClick.AddListener(OnMiddleClick);
            TutorialManager.Instance.rightButton.onClick.AddListener(OnRightClick);
            leftButtonTextObject = TutorialManager.Instance.leftButton.GetComponentInChildren<Text>();
            middleButtonTextObject = TutorialManager.Instance.middleButton.GetComponentInChildren<Text>();
            rightButtonTextObject = TutorialManager.Instance.rightButton.GetComponentInChildren<Text>();
            leftButtonTextObject.text = leftButtonText;
            middleButtonTextObject.text = middleButtonText;
            rightButtonTextObject.text = rightButtonText;
        }

        public void OnRightClick()
        {
            if (ifRight == 100)
            {
                TutorialManager.Instance.CompletedAllTutorials();
                ResetButtons();
            }
            else if (ifRight == 0)
            {
                TutorialManager.Instance.PreviousTutorial();
                ResetButtons();
            }
            else
            {
                TutorialManager.Instance.SetNextTutorial(ifRight);
                ResetButtons();
            }

            TutorialManager.Choice = ifRightChoice;
        }

        public void OnMiddleClick()
        {
            TutorialManager.Instance.SetNextTutorial(ifMiddle);
            ResetButtons();
            TutorialManager.Choice = ifMiddleChoice;
        }

        public void OnLeftClick()
        {
            if (ifLeft == 100)
            {
                TutorialManager.Instance.CompletedAllTutorials();
                ResetButtons();
            }
            else if (ifLeft == 0)
            {
                TutorialManager.Instance.PreviousTutorial();
                ResetButtons();
            }
            else
            {
                TutorialManager.Instance.SetNextTutorial(ifLeft);
                ResetButtons();
            }

            TutorialManager.Choice = ifLeftChoice;
        }

        public void ResetButtons()
        {
            TutorialManager.Instance.leftButton.onClick.RemoveAllListeners();
            TutorialManager.Instance.middleButton.onClick.RemoveAllListeners();
            TutorialManager.Instance.rightButton.onClick.RemoveAllListeners();
            TutorialManager.Instance.leftButton.onClick.AddListener(TutorialManager.Instance.PreviousTutorial);
            TutorialManager.Instance.rightButton.onClick.AddListener(TutorialManager.Instance.NextTutorial);
            leftButtonTextObject.text = "BACK";
            rightButtonTextObject.text = "NEXT";
        }
    }
}

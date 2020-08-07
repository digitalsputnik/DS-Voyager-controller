using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.UI.Overlays;
using VoyagerApp.Workspace;

namespace VoyagerApp.UI.Menus
{
    public class TutorialManager : Menu
    {
        public List<Tutorial> tutorials = new List<Tutorial>();

        [Space(10)]

        #region TutorialObjects

        [SerializeField] public InspectorMenuContainer inspectorMenuContainer = null;
        [SerializeField] public MenuContainer menuContainer = null;
        [SerializeField] Menu startMenu = null;

        [SerializeField] CanvasGroup canvas = null; 

        public GameObject rightMenuOverlay = null;
        public GameObject leftMenuOverlay = null;
        public GameObject topCenterOverlay = null;
        public GameObject defaultCenterOverlay = null;
        public GameObject extendedInfoField = null;
        public GameObject extendedToggle = null;
        public GameObject extendedToggleContainer = null;
        public GameObject middleButtonContainer = null;
        public Button middleButton = null;
        public Button leftButton = null;
        public Button rightButton = null;

        public Text title = null;
        public Text info = null;
        public Text extendedInfo = null;
        public Text topTitle = null;
        public Text topInfo = null;

        #endregion

        public bool setup = false;

        List<int> customOrder = new List<int>();

        private static TutorialManager instance = null;
        public static TutorialManager Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<TutorialManager>();

                if (instance == null)
                    Debug.LogError("TutorialManager instance is null");

                return instance;
            }
        }

        private Tutorial currentTutorial = null;

        void Awake()
        {
            Instance.leftButton.onClick.AddListener(Instance.PreviousTutorial);
            Instance.rightButton.onClick.AddListener(Instance.NextTutorial);
        }
        internal override void OnShow()
        {
            canvas.alpha = 1.0f;
            canvas.interactable = true;
            canvas.blocksRaycasts = true;

            Instance.menuContainer.ShowMenu(startMenu);
            Instance.inspectorMenuContainer.ShowMenu(null);

            if (PlayerPrefs.HasKey("TutorialDone"))
                SetNextTutorial(1);
            else
                SetNextTutorial(0);

            PlayerPrefs.SetString("TutorialDone", "true");
        }

        internal override void OnHide()
        {
            canvas.alpha = 0.0f;
            canvas.interactable = false;
            canvas.blocksRaycasts = false;

            Instance.menuContainer.ShowMenu(startMenu);
            Instance.inspectorMenuContainer.ShowMenu(null);
        }

        void SetOverlayData()
        {
            ResetValues();

            if (currentTutorial.centerOverlay != Tutorial.CenterOverlay.Top)
            {
                title.text = currentTutorial.title;
                info.text = currentTutorial.info;
                extendedInfo.text = currentTutorial.moreInfo;
                defaultCenterOverlay.SetActive(true);
            }
            
            if (currentTutorial.centerOverlay == Tutorial.CenterOverlay.MoreInfo)
            {
                middleButtonContainer.SetActive(false);
                extendedToggle.SetActive(true);
                extendedToggleContainer.SetActive(true);
            }
            else if (currentTutorial.centerOverlay == Tutorial.CenterOverlay.Prompt)
            {
                extendedToggleContainer.SetActive(false);
                middleButton.transform.gameObject.SetActive(true);
                middleButtonContainer.SetActive(true);
            }
            else if (currentTutorial.centerOverlay == Tutorial.CenterOverlay.Top)
            {
                topTitle.text = currentTutorial.title;
                topInfo.text = currentTutorial.info;
                topCenterOverlay.SetActive(true);
            }
        }

        void ResetValues()
        {
            if (currentTutorial.clearWorkspace)
                WorkspaceManager.instance.Clear();

            extendedInfoField.SetActive(false);
            defaultCenterOverlay.SetActive(false);
            topCenterOverlay.SetActive(false);
            extendedToggle.SetActive(false);
            middleButton.transform.gameObject.SetActive(false);
            rightButton.transform.gameObject.SetActive(!currentTutorial.disableRightButton);
            leftButton.transform.gameObject.SetActive(!currentTutorial.disableLeftButton);

            extendedToggle.GetComponentInChildren<Text>().text = "More Info";
            rightButton.onClick.RemoveAllListeners();
            rightButton.onClick.AddListener(NextTutorial);
            rightButton.GetComponentInChildren<Text>().text = "NEXT";
        }

        public void OpenMoreInfoClick()
        {
            extendedInfoField.SetActive(!extendedInfoField.activeSelf);
            extendedToggle.GetComponentInChildren<Text>().text = extendedInfoField.activeSelf ? "Less Info" : "More Info";
        }

        void Update()
        {
            if (currentTutorial)
                currentTutorial.CheckForAction();
        }

        public void NextTutorial()
        {
            SetNextTutorial(currentTutorial.Order + 1);
        }

        public void PreviousTutorial()
        {
            int jump = currentTutorial.backJump;

            SetNextTutorial(customOrder[customOrder.Count - 2 - jump]);

            for (int i = 0; i < jump + 2; i++)
                customOrder.RemoveAt(customOrder.Count - 1);

            if (currentTutorial.rightMenuToOpen != null)
                Instance.menuContainer.ShowMenu(currentTutorial.rightMenuToOpen);
            else
                Instance.menuContainer.ShowMenu(startMenu);

            if (currentTutorial.leftMenuToOpen != null)
                Instance.inspectorMenuContainer.ShowMenu(currentTutorial.leftMenuToOpen);
            else
                Instance.inspectorMenuContainer.ShowMenu(null);
        }

        public void SetNextTutorial(int currentOrder)
        {
            currentTutorial = GetTutorialByOrder(currentOrder);

            setup = false;

            if (!currentTutorial)
            {
                CompletedAllTutorials();
                return;
            }

            SetOverlayData();

            leftMenuOverlay.SetActive(currentTutorial.leftMenu);
            rightMenuOverlay.SetActive(currentTutorial.rightMenu);

            customOrder.Add(currentOrder);
        }

        public void CompletedAllTutorials()
        {
            OnHide();
            DialogBox.ResumeDialogues();
        }

        public Tutorial GetTutorialByOrder(int order)
        {
            for (int i = 0; i < tutorials.Count; i++)
            {
                if (tutorials[i].Order == order)
                    return tutorials[i];
            }

            return null;
        }
    }
}
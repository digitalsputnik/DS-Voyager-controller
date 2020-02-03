using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VoyagerApp.UI.Overlays
{
    public class DialogBox : MonoBehaviour
    {
        #region Singleton
        static DialogBox instance;
        void Awake() => instance = this;
        #endregion

        [SerializeField] Text titleText = null;
        [SerializeField] Text explanationText = null;
        [SerializeField] GameObject buttonPrefab;
        [SerializeField] RectTransform buttonContainer;

        List<Button> buttons = new List<Button>();

        CanvasGroup canvas;
        Queue<DialogBoxSettings> dialogues = new Queue<DialogBoxSettings>();

        void Start()
        {
            RectTransform rect = GetComponent<RectTransform>();
            rect.offsetMin = new Vector2(0.0f, 0.0f);
            rect.offsetMax = new Vector2(0.0f, 0.0f);

            canvas = GetComponent<CanvasGroup>();
            Hide();
        }

        public static void Show(
            string title, string explanation,
            string[] btns, Action[] onBtns)
        {
            MainThread.Dispach(() =>
            {
                DialogBoxSettings settings = new DialogBoxSettings(
                    title, explanation, btns, onBtns);
                instance.dialogues.Enqueue(settings);
                instance.ShowNextDialogue();
            });
        }

        void ShowNextDialogue()
        {
            if (!canvas.interactable && dialogues.Count > 0)
                ShowDialog(dialogues.Dequeue());
        }

        void ShowDialog(DialogBoxSettings settings) 
        {
            titleText.text = settings.title;
            explanationText.text = settings.explanation;

            for (int i = 0; i < settings.btns.Length; i++)
            {
                int copy = i;//Closure problem
                GameObject buttonObject = Instantiate(buttonPrefab, buttonContainer);
                Button button = buttonObject.GetComponent<Button>();
                button.GetComponentInChildren<Text>().text = settings.btns[i];
                button.onClick.AddListener(() => {
                    settings.onBtns[copy]?.Invoke();
                    DialogObserved();
                });
                buttons.Add(button);
            }

            Show();
        }

        void RemoveDialogBoxButtons() 
        {
            foreach (var button in buttons)
            {
                button.onClick.RemoveAllListeners();
                Destroy(button.gameObject);
            }
            buttons = new List<Button>();
        }

        void DialogObserved()
        {
            Hide();
            RemoveDialogBoxButtons();
            ShowNextDialogue();
        }

        void Show()
        {
            canvas.alpha = 1.0f;
            canvas.interactable = true;
            canvas.blocksRaycasts = true;
        }

        void Hide()
        {
            canvas.alpha = 0.0f;
            canvas.interactable = false;
            canvas.blocksRaycasts = false;
        }

        class DialogBoxSettings
        {
            public string title;
            public string explanation;
            public string[] btns;
            public Action[] onBtns;

            public DialogBoxSettings(
                string title, string explanation, string[] btns,
                Action[] onBtns)
            {
                this.title = title;
                this.explanation = explanation;
                this.btns = btns;
                this.onBtns = onBtns;
            }
        }
    }
}
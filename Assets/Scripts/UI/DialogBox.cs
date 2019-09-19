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
        [SerializeField] Button button1 = null;
        [SerializeField] Button button2 = null;

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
            string title, string explanation, string btn1,
            string btn2, Action onBtn1, Action onBtn2)
        {
            MainThread.Dispach(() =>
            {
                DialogBoxSettings settings = new DialogBoxSettings(
                    title, explanation, btn1, btn2, onBtn1, onBtn2);
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

            button1.onClick.RemoveAllListeners();
            button1.GetComponentInChildren<Text>().text = settings.btn1;
            button1.onClick.AddListener(() => {
                settings.onBtn1.Invoke();
                DialogObserved();
            });

            button2.onClick.RemoveAllListeners();
            button2.GetComponentInChildren<Text>().text = settings.btn2;
            button2.onClick.AddListener(() => {
                settings.onBtn2.Invoke();
                DialogObserved();
            });

            Show();
        }

        void DialogObserved()
        {
            Hide();
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
            public string btn1;
            public string btn2;
            public Action onBtn1;
            public Action onBtn2;

            public DialogBoxSettings(
                string title, string explanation, string btn1,
                string btn2, Action onBtn1, Action onBtn2)
            {
                this.title = title;
                this.explanation = explanation;
                this.btn1 = btn1;
                this.btn2 = btn2;
                this.onBtn1 = onBtn1;
                this.onBtn2 = onBtn2;
            }
        }
    }
}
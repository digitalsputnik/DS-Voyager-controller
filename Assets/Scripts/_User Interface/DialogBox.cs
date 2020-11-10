using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VoyagerController.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class DialogBox : MonoBehaviour
    {
        #region Singleton
        private static DialogBox _instance;
        private void Awake() => _instance = this;
        #endregion

        [SerializeField] private Text _titleText = null;
        [SerializeField] private Text _explanationText = null;
        [SerializeField] private TextButton _buttonPrefab = null;
        [SerializeField] private RectTransform _buttonsContainer = null;
        
        private readonly List<TextButton> _buttons = new List<TextButton>();
        private readonly Queue<DialogSettings> _dialogues = new Queue<DialogSettings>();

        private CanvasGroup _canvas;
        private bool _paused;

        private void Start()
        {
            var rect = GetComponent<RectTransform>();
            rect.offsetMin = new Vector2(0.0f, 0.0f);
            rect.offsetMax = new Vector2(0.0f, 0.0f);

            _canvas = GetComponent<CanvasGroup>();
            Hide();
        }

        public static bool Paused
        {
            get => _instance._paused;
            set
            {
                _instance._paused = value;
                if (!_instance._paused)
                    _instance.ShowNextDialog();
            }
        }

        public static void Show(string title, string explanation, string[] titles, Action[] actions)
        {
            MainThread.Dispatch(() =>
            {
                var settings = new DialogSettings(title, explanation, titles, actions);
                _instance._dialogues.Enqueue(settings);
                if (!Paused) _instance.ShowNextDialog();
            });
        }

        private void ShowNextDialog()
        {
            if (!_canvas.interactable && _dialogues.Count > 0)
                ShowDialog(_dialogues.Dequeue());
        }

        private void ShowDialog(DialogSettings dialog)
        {
            _titleText.text = dialog.Title;
            _explanationText.text = dialog.Explanation;

            for (var i = 0; i < dialog.ButtonTitles.Length; i++)
            {
                var button = Instantiate(_buttonPrefab, _buttonsContainer);
                var title = dialog.ButtonTitles[i];
                var action = dialog.ButtonActions[i];
                button.Setup(title, () =>
                {
                    action?.Invoke();
                    DialogFinished();
                });
                _buttons.Add(button);
            }

            Show();
        }

        private void DialogFinished()
        {
            Hide();
            ClearButtons();
            ShowNextDialog();
        }

        private void ClearButtons()
        {
            foreach (var button in _buttons)
                Destroy(button.gameObject);
        }

        private void Show()
        {
            _canvas.alpha = 1.0f;
            _canvas.interactable = true;
            _canvas.blocksRaycasts = true;
        }

        private void Hide()
        {
            _canvas.alpha = 0.0f;
            _canvas.interactable = false;
            _canvas.blocksRaycasts = false;
        }

        private class DialogSettings
        {
            public string Title { get; }
            public string Explanation { get; }
            public string[] ButtonTitles { get; }
            public Action[] ButtonActions { get; }

            public DialogSettings(string title, string explanation, string[] titles, Action[] actions)
            {
                Title = title;
                Explanation = explanation;
                ButtonTitles = titles;
                ButtonActions = actions;
            }
        }
    }
}
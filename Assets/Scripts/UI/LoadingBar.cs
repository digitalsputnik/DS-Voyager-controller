using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VoyagerApp.UI.Overlays
{
    public class LoadingBar : MonoBehaviour
    {
        #region Instance
        static LoadingBar instance;
        void Awake() => instance = this;
        #endregion

        [SerializeField] Text titleText = null;
        [SerializeField] Image barImage = null;

        CanvasGroup canvas;
        Queue<LoadingBarProcess> processes = new Queue<LoadingBarProcess>();
        LoadingBarProcess activeProcess;

        void Start()
        {
            RectTransform rect = GetComponent<RectTransform>();
            rect.offsetMin = new Vector2(0.0f, 0.0f);
            rect.offsetMax = new Vector2(0.0f, 0.0f);

            canvas = GetComponent<CanvasGroup>();
            Hide();
        }

        public static LoadingBarProcess CreateLoadProcess(string title)
        {
            LoadingBarProcess process = new LoadingBarProcess(instance, title);
            instance.processes.Enqueue(process);
            instance.ShowNextLoadingProcess();
            return process;
        }

        public void CancelBtnClicked()
        {
            activeProcess.Cancel();
            LoadingProcessFinished(activeProcess);
        }

        internal void UpdateTitle(string title)
        {
            titleText.text = title;
        }

        internal void UpdateProgress(float normalized)
        {
            barImage.fillAmount = normalized;
        }

        internal void LoadingProcessFinished(LoadingBarProcess process)
        {
            if (process.active) Hide();
            ShowNextLoadingProcess();
        }

        void ShowNextLoadingProcess()
        {
            if (!canvas.interactable && processes.Count > 0)
                ShowLoadingProcess(processes.Dequeue());
        }

        void ShowLoadingProcess(LoadingBarProcess process)
        {
            UpdateTitle(process.title);
            UpdateProgress(process.normalized);

            activeProcess = process;
            process.active = true;
            Show();
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
    }

    public class LoadingBarProcess
    {
        LoadingBar loadingBar;
        public float normalized { get; private set; }
        public string title { get; private set; }
        internal bool active;

        public Action onCancel;

        internal LoadingBarProcess(LoadingBar loadingBar, string title)
        {
            this.loadingBar = loadingBar;
            this.title = title;
            normalized = 0.0f;
        }

        internal void Cancel()
        {
            onCancel?.Invoke();
        }

        public void UpdateProgress(float normalized)
        {
            this.normalized = normalized;
            if (active) loadingBar.UpdateProgress(normalized);
            if (normalized >= 0.99f)
                loadingBar.LoadingProcessFinished(this);
        }

        public void UpdateTitle(string title)
        {
            this.title = title;
            if (active) loadingBar.UpdateTitle(title);
        }
    }
}
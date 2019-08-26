using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VoyagerApp.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Menu : MonoBehaviour
    {
        protected CanvasGroup canvasGroup;

        public bool Open
        {
            get => canvasGroup.interactable;
            set
            {
                if (value != Open)
                {
                    if (value)
                        OnShow();
                    else
                        OnHide();

                    ChangeCanvasGroup(value);
                }
            }
        }

        public virtual void Start()
        {
            canvasGroup = GetComponent<CanvasGroup>();

            SetPosition();
            ChangeCanvasGroup(false);
        }

        void SetPosition()
        {
            RectTransform rect = GetComponent<RectTransform>();
            rect.offsetMin = new Vector2(0.0f, 0.0f);
            rect.offsetMax = new Vector2(0.0f, 0.0f);
        }

        void ChangeCanvasGroup(bool value)
        {
            canvasGroup.interactable = value;
            canvasGroup.blocksRaycasts = value;
            canvasGroup.alpha = value ? 1.0f : 0.0f;
        }

        internal virtual void OnShow() { }
        internal virtual void OnHide() { }
    }
}

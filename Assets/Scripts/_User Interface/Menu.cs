using UnityEngine;

namespace VoyagerController.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Menu : MonoBehaviour
    {
        protected CanvasGroup CanvasGroup;

        public bool Open
        {
            get => CanvasGroup.interactable;
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
            CanvasGroup = GetComponent<CanvasGroup>();

            SetPosition();
            ChangeCanvasGroup(false);
        }

        private void SetPosition()
        {
            var rect = GetComponent<RectTransform>();
            rect.offsetMin = new Vector2(0.0f, 0.0f);
            rect.offsetMax = new Vector2(0.0f, 0.0f);
        }

        private void ChangeCanvasGroup(bool value)
        {
            CanvasGroup.interactable = value;
            CanvasGroup.blocksRaycasts = value;
            CanvasGroup.alpha = value ? 1.0f : 0.0f;
        }

        internal virtual void OnShow() { }
        internal virtual void OnHide() { }
    }
}
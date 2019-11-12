using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace VoyagerApp.UI
{
    public class ShowHideMenu : MonoBehaviour
    {
        [Header("General")]
        [SerializeField] RectTransform target   = null;
        public float speed            = 0.0f;
        public Vector2 openPosition             = Vector2.zero;
        public Vector2 closedPosition           = Vector2.zero;
        [SerializeField] bool open              = true;
        [Header("Icon")]
        [SerializeField] Sprite showSprite      = null;
        [SerializeField] Sprite hideSprite      = null;
        [SerializeField] Image iconImage        = null;

        void Start()
        {
            Open = open;
            StopAllCoroutines();
            var dest = open ? openPosition : closedPosition;
            target.anchoredPosition = dest;
        }

        public bool Open
        {
            get => open;
            set
            {
                open = value;
                iconImage.sprite = open ? hideSprite : showSprite;
                PlayAnimation();
            }
        }

        public void Toggle()
        {
            Open = !Open;
        }

        void PlayAnimation()
        {
            StopAllCoroutines();
            StartCoroutine(IEnumPlayAnimation());
        }

        IEnumerator IEnumPlayAnimation()
        {
            float startTime = Time.time;
            float endTime = startTime + speed;

            Vector2 startPosition = target.anchoredPosition;
            Vector2 destPosition = open ? openPosition : closedPosition;

            while (target.anchoredPosition != destPosition)
            {
                float passed = Time.time - startTime;
                float time = passed / speed;
                target.anchoredPosition = Vector2.Lerp(startPosition,
                                                       destPosition,
                                                       time);
                yield return null;
            }
        }
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Effects;
using VoyagerApp.Utilities;

namespace VoyagerApp.UI.Menus
{
    public class SetEffectItem : MonoBehaviour
    {
        [SerializeField] RawImage thumbnailImage = null;
        [SerializeField] UnityEngine.UI.Image loadingOverlay = null;
        [SerializeField] Button removeButton = null;
        [SerializeField] Text nameText = null;
        [SerializeField] Text infoText = null;

        public Effect effect;

        Button button;

        void Awake()
        {
            loadingOverlay.gameObject.SetActive(true);
            button = GetComponent<Button>();
            button.interactable = false;
        }

        public void SetEffect(Effect effect)
        {
            this.effect = effect;

            if (effect.available.value)
                SetupUI();
            else
                StartCoroutine(WaitForAvailable());
        }

        public void Remove()
        {
            GetComponentInParent<SetEffectMenu>().RemoveEffect(effect);
        }

        public void Select()
        {
            GetComponentInParent<SetEffectMenu>().SelectEffect(effect);
        }

        public void StartResizing()
        {
            loadingOverlay.gameObject.SetActive(true);
            loadingOverlay.gameObject.GetComponentInChildren<Text>().text = "RESIZING";
            button.interactable = false;
        }

        public void StopResizing(Effect effect)
        {
            loadingOverlay.gameObject.SetActive(false);
            button.interactable = true;
            this.effect = effect;
            SetupUI();
        }

        IEnumerator WaitForAvailable()
        {
            yield return new WaitUntil(() => effect.available.value);
            SetupUI();
        }

        void SetupUI()
        {
            loadingOverlay.gameObject.SetActive(false);
            thumbnailImage.texture = effect.thumbnail;
            nameText.text = effect.name;
            button.interactable = true;

            if (effect is Video video)
            {
                infoText.text =
                    "duration \n" +
                    TimeUtils.GetVideoTimecode(video) + "\n" +
                    "resolution \n" +
                    video.width + "x" + video.height;
            }

            if (effect.preset)
                removeButton.gameObject.SetActive(false);
        }
    }
}
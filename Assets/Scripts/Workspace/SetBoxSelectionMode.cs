using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VoyagerApp.UI
{
    public class SetBoxSelectionMode : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] Color pressedColor = Color.white;
        [SerializeField] Color releasedColor = Color.white;
        [SerializeField] Image modeImage = null;
        [SerializeField] Sprite[] modes = null;

        int index = 2;

        void Start()
        {
            if (!Application.isMobilePlatform)
            {
                gameObject.SetActive(false);
                return;
            }

            OnPointerUp(null);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            index++;
            index = index % modes.Length;
            modeImage.color = pressedColor;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            modeImage.sprite = modes[index];
            BoxSelection.mode = (BoxSelectionMode)index;
            modeImage.color = releasedColor;
        }
    }
}
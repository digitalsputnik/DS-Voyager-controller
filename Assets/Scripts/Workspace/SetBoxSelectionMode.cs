using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VoyagerApp.UI
{
    public class SetBoxSelectionMode : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] Color pressedColor = Color.white;
        [SerializeField] Color releasedColor = Color.white;
        [SerializeField] Text modeText = null;
        [SerializeField] string[] modes;

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
            modeText.color = pressedColor;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            modeText.text = $"BOX SELECTION MODE: {modes[index]}";
            BoxSelection.mode = (BoxSelectionMode)index;
            modeText.color = releasedColor;
        }
    }
}
using UnityEngine;
using VoyagerApp.Workspace;

namespace VoyagerApp.UI.Menus
{
    public class SliderMenu : Menu
    {

        [SerializeField] CanvasGroup SliderRect = null;

        [SerializeField] CanvasGroup sliderCanvasGroup = null;
        public GameObject[] disableObject;


        public override void Start()
        {
            base.Start();

            var rect = SliderRect.GetComponent<RectTransform>();
            rect.offsetMin = new Vector2(0.0f, 0.0f);
            rect.offsetMax = new Vector2(0.0f, 0.0f);

        }

        public void DisableGO()
        {
            foreach (GameObject GO in disableObject)
            {
                GO.SetActive(false);
            }
        }


        public void EnableSliderMenu()
        {
            sliderCanvasGroup.interactable = true;
            sliderCanvasGroup.blocksRaycasts = true;
            sliderCanvasGroup.alpha = 1;

            //sliderOption.SetActive(true);
        }

        public void DisableSliderMenu()
        {
            sliderCanvasGroup.interactable = false;
            sliderCanvasGroup.blocksRaycasts = false;
            sliderCanvasGroup.alpha = 0;

           // sliderOption.SetActive(false);
        }
    }
}
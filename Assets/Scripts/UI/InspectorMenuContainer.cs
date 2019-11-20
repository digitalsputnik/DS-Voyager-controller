using System.Collections;
using UnityEngine;

namespace VoyagerApp.UI
{
    public class InspectorMenuContainer : MenuContainer
    {
        [SerializeField] ShowHideMenu showHide = null;

        internal override void Start()
        {
            base.Start();
            if (Screen.cutouts.Length > 0)
                showHide.openPosition.x += Screen.cutouts[0].width;
        }

        public override void ShowMenu(Menu menu)
        {
            showHide.Open = menu != null;
            base.ShowMenu(menu);
        }

        public void HideCurrentMenu() => ShowMenu(null);

        public void CloseInHided()
        {
            if (!showHide.Open)
                StartCoroutine(CloseInSeconds());
        }

        IEnumerator CloseInSeconds()
        {
            yield return showHide.speed;
            ShowMenu(null);
        }
    }
}

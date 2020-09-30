using System.Collections;
using UnityEngine;

namespace VoyagerController.UI
{
    public class InspectorMenuContainer : MenuContainer
    {
        [SerializeField] private ShowHideMenu _showHide = null;

        internal override void Start()
        {
            base.Start();

            if (Screen.cutouts.Length > 0)
            {
                var pos = _showHide.OpenPosition;
                pos.x += Screen.cutouts[0].width;
                _showHide.OpenPosition = pos;
            }
        }

        public override void ShowMenu(Menu menu)
        {
            _showHide.Open = menu != null;
            base.ShowMenu(menu);
        }

        public void HideCurrentMenu() => ShowMenu(null);

        public void CloseInHidden()
        {
            if (!_showHide.Open)
                StartCoroutine(CloseInSeconds());
        }

        private IEnumerator CloseInSeconds()
        {
            yield return _showHide.Speed;
            ShowMenu(null);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

namespace VoyagerController.UI
{
    public class AboutMenu : Menu
    {
        [SerializeField] private Text _versionText = null;

        internal override void OnShow()
        {
            _versionText.text = "App " + Application.version + "\nLamp software " + VoyagerUpdater.Version;
        }
    }
}
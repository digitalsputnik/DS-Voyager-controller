using System;
using UnityEngine;
using UnityEngine.UI;

namespace VoyagerController.UI
{
    public class AboutMenu : Menu
    {
        [SerializeField] private Text _versionText = null;
        [SerializeField] private Text _copyrightText = null;

        internal override void OnShow()
        {
            _versionText.text = "VERSION " + Application.version + "\nLAMP VERSION " + VoyagerUpdater.Version;
            _copyrightText.text = $"© {DateTime.Now.Year} - Digital Sputnik";
        }

        public void OpenHelp()
        {
            Application.OpenURL(ApplicationSettings.HELP_URL);
        }
    }
}
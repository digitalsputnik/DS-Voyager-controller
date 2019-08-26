using System;
using UnityEngine;
using UnityEngine.UI;

namespace VoyagerApp.UI.Menus
{
    public class AboutMenu : Menu
    {
        [SerializeField] Text versionText   = null;
        [SerializeField] Text copyrightText = null;

        internal override void OnShow()
        {
            string version = Application.version;
            versionText.text = $"VERSION {version}";

            int year = DateTime.Now.Year;
            copyrightText.text = $"© {year} - Digital Sputnik";
        }
    }
}
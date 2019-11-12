using System;
using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Lamps;

namespace VoyagerApp.UI.Menus
{
    public class AboutMenu : Menu
    {
        [SerializeField] Text versionText   = null;
        [SerializeField] Text copyrightText = null;

        internal override void OnShow()
        {
            string version = Application.version;
            versionText.text =
                $"VERSION {version}\n" +
                $"LAMP VERSION {UpdateSettings.VoyagerAnimationVersion}";

            int year = DateTime.Now.Year;
            copyrightText.text = $"© {year} - Digital Sputnik";
        }
    }
}
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;

namespace VoyagerApp.UI.Menus
{
    public class SaveMenu : Menu
    {
        [SerializeField] InputField filenameField   = null;
        [SerializeField] Button saveButton          = null;

        internal override void OnShow()
        {
            filenameField.onValueChanged.AddListener(FilenameFieldChanged);
            // ToShortDateString confuses the program to find paths because of '/' between dates
            filenameField.text = $"workspace_{DateTime.Now.ToLongDateString()}_" +
                                 $"{DateTime.Now.ToLongTimeString()}".Replace(".", "_");
        }

        internal override void OnHide()
        {
            filenameField.onValueChanged.RemoveListener(FilenameFieldChanged);
        }

        void FilenameFieldChanged(string file)
        {
            saveButton.interactable = file.Length > 0;
        }

        public void Save()
        {
            var items = WorkspaceManager.instance.Items.ToArray();
            WorkspaceSaveLoad.Save(filenameField.text, items);
            GetComponentInParent<InspectorMenuContainer>().ShowMenu(null);
        }

        public void SaveVideoMapping()
        {
            if (!Directory.Exists(FileUtils.WorkspaceSavesPath + "/vm"))
                Directory.CreateDirectory(FileUtils.WorkspaceSavesPath + "/vm");
            var items = WorkspaceManager.instance.Items.ToArray();
            WorkspaceSaveLoad.Save("vm/" + filenameField.text, items);
            GetComponentInParent<InspectorMenuContainer>().ShowMenu(null);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Projects;
using VoyagerApp.UI.Overlays;
using VoyagerApp.Utilities;

namespace VoyagerApp.UI.Menus
{
    public class SaveMenu : Menu
    {
        [SerializeField] InputField filenameField   = null;
        [SerializeField] Button saveButton          = null;

        internal override void OnShow()
        {
            filenameField.onValueChanged.AddListener(FilenameFieldChanged);
            filenameField.text = $"save_{Random.Range(0, 1000)}";
            FilenameFieldChanged(filenameField.text);
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
            if (Project.Save(filenameField.text) != null)
                GetComponentInParent<InspectorMenuContainer>().ShowMenu(null);
        }

        public void Export()
        {
            Project.Export(filenameField.text, OnExportReady);
        }

        void OnExportReady(bool success, string path)
        {
            if (success)
            {
                FileUtils.SaveProject(path, filenameField.text);
                GetComponentInParent<InspectorMenuContainer>().ShowMenu(null);
            }
            else
            {
                DialogBox.Show(
                    "ERROR",
                    "Error packing the project to export it.",
                    "TRY AGAIN", "CANCEL",
                    Export, null);
            }
        }
    }
}

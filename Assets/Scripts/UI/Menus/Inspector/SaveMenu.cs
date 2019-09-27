using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Projects;
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
            Project.Save(filenameField.text);
            GetComponentInParent<InspectorMenuContainer>().ShowMenu(null);
        }

        public void Export()
        {
            string path = Project.Export("export");
            bool success = FileUtils.SaveProject(path, filenameField.text);
            GetComponentInParent<InspectorMenuContainer>().ShowMenu(null);
        }
    }
}

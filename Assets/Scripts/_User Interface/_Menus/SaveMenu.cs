using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VoyagerController.ProjectManagement;

namespace VoyagerController.UI
{
    public class SaveMenu : Menu
    {
        [SerializeField] InputField filenameField = null;
        [SerializeField] Button saveButton = null;

        internal override void OnShow()
        {
            filenameField.onValueChanged.AddListener(FilenameFieldChanged);
            filenameField.text = $"save_{UnityEngine.Random.Range(0, 1000)}";
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
            var projPath = Project.ProjectsDirectory;
            var projects = Directory.GetDirectories(projPath).Select(Path.GetFileName);
            var projectName = filenameField.text;

            if (projects.Any(p => p == projectName))
            {
                DialogBox.Show(
                    "WARNING",
                    $"Project \"{projectName}\" already exists. Overwrite?",
                    new[] { "OK", "CANCEL" },
                    new Action[] { () => ForceSave(projectName), null }
                    );
            }
            else
            {
                ForceSave(projectName);
            }
        }

        private void ForceSave(string filename)
        {
            if (Project.Save(filename) != null)
                GetComponentInParent<InspectorMenuContainer>().ShowMenu(null);
        }

        public void Export()
        {
            //TODO
            //Project.Export(filenameField.text, OnExportReady);
        }

        void OnExportReady(bool success, string path)
        {
            /*if (success)
            {
                FileUtils.SaveProject(path, filenameField.text);
                GetComponentInParent<InspectorMenuContainer>().ShowMenu(null);
            }
            else
            {
                DialogBox.Show(
                    "ERROR",
                    "Error packing the project to export it.",
                    new string[] { "TRY AGAIN", "CANCEL" },
                    new Action[] { Export, null });
            }*/
        }
    }
}
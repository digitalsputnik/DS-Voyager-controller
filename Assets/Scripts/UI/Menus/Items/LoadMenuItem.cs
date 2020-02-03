﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Projects;
using VoyagerApp.UI.Overlays;
using VoyagerApp.Utilities;

namespace VoyagerApp.UI.Menus
{
    public class LoadMenuItem : MonoBehaviour
    {
        public Button button;

        [SerializeField] Text nameText  = null;
        [SerializeField] Text lampsText = null;

        string path;

        void Start()
        {
            button = GetComponent<Button>();
        }

        public void SetPath(string path)
        {
            this.path = path;
            Task.Run(LoadProject);
        }

        async Task LoadProject()
        {
            try
            {
                var file = Path.Combine(path, Project.PROJECT_FILE);
                if (!File.Exists(file))
                    throw new Exception("Corrupt project");

                var json = await FileUtils.ReadAllTextAsync(file);

                if (!FileUtils.IsJsonValid(json))
                    throw new Exception("Corrupt project");

                var data = Project.GetProjectData(json);

                if (data == null)
                    throw new Exception("Error on loading");

                int lamps = data.items.Where(i => i.type == "voyager_lamp").Count();

                MainThread.Dispach(() =>
                {
                    nameText.text = Path.GetFileName(path);
                    lampsText.text = $"LAMPS: {lamps}";
                    GetComponent<Button>().interactable = true;
                });
            }
            catch (Exception ex)
            {
                MainThread.Dispach(() =>
                {
                    Debug.LogError(ex);
                    nameText.text = Path.GetFileName(path);
                    lampsText.text = ex.Message.ToUpper();
                });
            }
        }

        public void Load()
        {
            string project = Path.GetFileName(path);
            GetComponentInParent<LoadMenu>().LoadProject(project);
            GetComponentInParent<InspectorMenuContainer>().ShowMenu(null);
        }

        public void Delete()
        {
            DialogBox.Show(
                "ARE YOU SURE?",
                "Are you sure you want to delete this project?",
                "CANCEL", "OK", null,
                () =>
                {
                    Directory.Delete(path, true);
                    GetComponentInParent<LoadMenu>().RemoveItem(this);
                });
        }
    }
}
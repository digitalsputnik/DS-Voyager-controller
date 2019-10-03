using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Projects;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.UI.Menus
{
    public class LoadMenuItem : MonoBehaviour
    {
        [SerializeField] Text nameText  = null;
        [SerializeField] Text lampsText = null;

        string path;

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
                {
                    Directory.Delete(path, true);
                    throw new Exception();
                }

                var settings = Project.JsonSettings();
                var json = await FileUtils.ReadAllTextAsync(file);

                if (!FileUtils.IsJsonValid(json))
                    throw new Exception();

                MainThread.Dispach(() =>
                {
                    var project = JsonConvert.DeserializeObject<Project>(json, settings);

                    var lampsCount = project
                        .items
                        .Where(i => i is LampItemSaveData)
                        .ToList()
                        .Count;

                    nameText.text = Path.GetFileName(path);
                    lampsText.text = $"LAMPS: {lampsCount}";
                    GetComponent<Button>().interactable = true;
                });
            }
            catch
            {
                MainThread.Dispach(() =>
                {
                    GetComponentInParent<LoadMenu>().RemoveItem(this);
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
            Directory.Delete(path, true);
            GetComponentInParent<LoadMenu>().RemoveItem(this);
        }
    }
}
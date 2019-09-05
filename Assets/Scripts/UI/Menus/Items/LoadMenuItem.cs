using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Projects;
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
            try
            {
                var file = Path.Combine(path, Project.PROJECT_FILE);

                if (!File.Exists(file))
                    throw new Exception($"File {file} doesn't exist!");

                var settings = Project.JsonSettings();
                var json = File.ReadAllText(file);
                var project = JsonConvert.DeserializeObject<Project>(json, settings);

                var lampsCount = project
                    .items
                    .Where(i => i is LampItemSaveData)
                    .ToList()
                    .Count;

                nameText.text = Path.GetFileName(path);
                lampsText.text = $"LAMPS: {lampsCount}";
                this.path = path;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                GetComponentInParent<LoadMenu>().RemoveItem(this);
            }
        }

        public void Load()
        {
            string project = Path.GetFileName(path);
            Project.Load(project);
            GetComponentInParent<InspectorMenuContainer>().ShowMenu(null);
        }

        // Should be used only by LoadMenu!
        public void Remove()
        {
            Destroy(gameObject);
        }

        public void Delete()
        {
            Directory.Delete(path, true);
            GetComponentInParent<LoadMenu>().RemoveItem(this);
        }
    }
}
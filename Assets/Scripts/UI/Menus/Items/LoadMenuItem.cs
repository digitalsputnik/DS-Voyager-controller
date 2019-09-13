using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Lamps;
using VoyagerApp.Projects;
using VoyagerApp.UI.Overlays;
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
            //LampManager.instance.Lamps.Clear();

            string project = Path.GetFileName(path);
            Project.Load(project);

            DialogBox.Show(
                "Send loaded video buffer to lamps?",
                "Clicking \"Ok\" will send loaded video to lamps, otherwise " +
                "Only lamp positions will be loaded, but lamps will still play " +
                "the video, they have at the moment.",
                "Cancel", "Ok",
                () => { },  // On cancel
                () => {     // On okey
                    var bufferSender = new ProjectBufferSender(
                        WorkspaceUtils.Lamps.ToArray(),
                        this);
                    bufferSender.StartSending();
                });

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
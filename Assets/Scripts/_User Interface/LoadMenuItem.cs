using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VoyagerController.ProjectManagement;

namespace VoyagerController.UI
{
    public class LoadMenuItem : MonoBehaviour
    {
        public Button button;
        public string fileName => Path.GetFileName(path);

        [SerializeField] Text nameText = null;
        [SerializeField] Text dateText = null;

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

                MainThread.Dispatch(() =>
                {
                    var videos = Path.Combine(path, Project.VIDEOS_DIRECTORY);
                    var settings = Project.ConstructJsonSettings(videos);
                    var project = JsonConvert.DeserializeObject<ProjectData>(json, settings);

                    var lamps = project.Lamps.Where(i => i != null).ToList();

                    if (lamps == null)
                        Debug.Log("lamps are null " + path);

                    nameText.text = fileName;
                    dateText.text = Directory.GetLastWriteTime(path).ToString(); //lamps != null ? $"LAMPS: {lamps.Count()}" : $"LAMPS: null";
                    GetComponent<Button>().interactable = true;
                });
            }
            catch (Exception ex)
            {
                MainThread.Dispatch(() =>
                {
                    Debug.LogError(path + " - " + ex, this);
                    nameText.text = Path.GetFileName(path);
                    dateText.text = ex.Message.ToUpper();
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
            Delete(null);
        }

        public void Delete(Action onDeleted = null)
        {
            DialogBox.Show(
                "ARE YOU SURE?",
                "Are you sure you want to delete this project?",
                new string[] { "CANCEL", "OK" },
                new Action[] {  null,
                    () =>
                    {
                        Directory.Delete(path, true);
                        GetComponentInParent<LoadMenu>().RemoveItem(this);
                        onDeleted?.Invoke();
                    }
                }
            );
        }
    }
}
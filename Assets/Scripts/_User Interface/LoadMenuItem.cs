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

                /*var data = Project.LoadDataOnly(path, json);

                if (data == null)
                    throw new Exception("Error on loading");

                var lamps = data.Lamps.Where(i => i != null).ToList();

                if (lamps == null)
                    Debug.Log("lamps are null " + path);*/

                MainThread.Dispatch(() =>
                {
                    nameText.text = fileName;
                    lampsText.text = $"LAMPS: {1}";
                    GetComponent<Button>().interactable = true;
                });
            }
            catch (Exception ex)
            {
                MainThread.Dispatch(() =>
                {
                    Debug.LogError(path + " - " + ex, this);
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
using System;
using System.IO;
using System.Linq;
using DigitalSputnik;
using UnityEngine;
using VoyagerController.UI;

namespace VoyagerController.ProjectManagement
{
    public class AutoLoad : MonoBehaviour
    {
        private void Start()
        {
            if (HasAnySavedProjects())
            {
                DialogBox.Show(
                    "LOAD PREVIOUS?",
                    "Do you want to load your previous project?",
                    new[] {"YES", "CANCEL"},
                    new Action[] {LoadPreviousProject, null});
            }
        }

        private static void LoadPreviousProject()
        {
            try
            {
                Project.Load(GetPreviousProject());
            }
            catch (Exception ex)
            {
                DialogBox.Show(
                    "ERROR LOADING PREVIOUS PROJECT",
                    "Something went wrong and previous project could not be loaded.",
                    new[] {"OK"},
                    new Action[] {null});
                
                DebugConsole.LogError($"[ERROR LOADING PREVIOUS PROJECT] {ex.Message}");
            }
        }

        private static bool HasAnySavedProjects()
        {
            return Directory.Exists(Project.ProjectsDirectory) && new DirectoryInfo(Project.ProjectsDirectory).GetDirectories().Any();
        }

        private static string GetPreviousProject()
        {
            return new DirectoryInfo(Project.ProjectsDirectory)
                .GetDirectories()
                .OrderByDescending(d=>d.LastWriteTimeUtc)
                .First()
                .Name;
        }
    }
}
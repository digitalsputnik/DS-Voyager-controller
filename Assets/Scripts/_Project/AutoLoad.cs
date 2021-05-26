﻿using System;
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
            if (HasAsked)
            {
                if (ApplicationSettings.AutoLoad)
                    LoadPreviousProject();
                return;
            }
            
            if (HasAnySavedProjects())
            {
                DialogBox.Show(
                    "LOAD PREVIOUS PROJECT?",
                    "This will be set as a default action. You can always change it under the settings.",
                    new[] {"YES", "NO"},
                    new Action[] {YesClicked, NoClicked});
            }
        }

        private static void YesClicked()
        {
            LoadPreviousProject();
            ApplicationSettings.AutoLoad = true;
            SetLoadAsked();
        }

        private static void NoClicked()
        {
            ApplicationSettings.AutoLoad = false;
            SetLoadAsked();
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

        private static bool HasAsked => PlayerPrefs.HasKey("auto_load_asked");

        private static void SetLoadAsked() => PlayerPrefs.SetInt("auto_load_asked", 1);
    }
}
using System;
using System.IO;
using Crosstales.FB;
using UnityEngine;

namespace VoyagerApp.Utilities
{
    public static class FileUtils
    {
        public static string WorkspaceSavesPath
        {
            get => Path.Combine(Application.persistentDataPath, "workspaces");
        }

        public static void LoadVideoFromDevice(PathHandler onLoaded)
        {
            if (Application.isMobilePlatform)
            {
                NativeGallery.GetVideoFromGallery((string path) =>
                {
                    onLoaded.Invoke(path == "" ? null : path);
                }, "", "video/*");
            }
            else
            {
                string documents = DocumentsPath;
                ExtensionFilter[] extensions = { new ExtensionFilter("Video", "mp4") };
                string path = FileBrowser.OpenSingleFile("Open Video", documents, extensions);
                onLoaded.Invoke(path == "" ? null : path);
            }
        }

        public static void LoadPictureFromDevice(PathHandler onLoaded)
        {
            if (Application.isMobilePlatform)
            {
                NativeGallery.GetImageFromGallery((string path) =>
                {
                    onLoaded.Invoke(path == "" ? null : path);
                }, "", "image/*");
            }
            else
            {
                string documents = DocumentsPath;
                ExtensionFilter[] extensions =
                {
                    new ExtensionFilter("PNG Picture", "png"),
                    new ExtensionFilter("JPEG Picture", "png")
                };
                string path = FileBrowser.OpenSingleFile("Open Picture", documents, extensions);
                onLoaded.Invoke(path == "" ? null : path);
            }
        }

        public static string DocumentsPath
        {
            get
            {
                if (Application.platform == RuntimePlatform.WindowsPlayer ||
                    Application.platform == RuntimePlatform.WindowsEditor)
                    return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                else
                    return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            }
        }

        public static string SelectionFilePath => "tmp/to_vm";
        public static string SelectionFilePathFull
		{
			get => Path.Combine(Application.persistentDataPath,
                                "workspaces",
                                SelectionFilePath + ".ws");
		}

		public static string WorkspaceStatePath => "tmp/from_vm";
        public static string WorkspaceStatePathFull
        {
            get => Path.Combine(Application.persistentDataPath,
                                "workspaces",
                                WorkspaceStatePath + ".ws");
        }

        public static string ProjectPath => Application.persistentDataPath;

        public static bool SaveProject(string path, string name)
        {
            if (Application.isMobilePlatform)
            {
                // TODO: Implement!
                return false;
            }
            else
            {
                string documents = DocumentsPath;
                ExtensionFilter[] extensions = { new ExtensionFilter("Voyager Controller Project", "vcp") };
                string to = FileBrowser.SaveFile("Save Project", documents, $"{name}.vcp", extensions);
                File.Copy(path, to);
                return true;
            }
        }

        public static void LoadProject(PathHandler onLoaded)
        {
            if (Application.isMobilePlatform)
            {
                // TODO: Implement!
            }
            else
            {
                string documents = DocumentsPath;
                ExtensionFilter[] extensions = { new ExtensionFilter("Voyager Controller Project", "vcp") };
                string path = FileBrowser.OpenSingleFile("Open Project", documents, extensions);
                onLoaded.Invoke(path == "" ? null : path);
            }
        }
    }

    public delegate void PathHandler(string path);
}
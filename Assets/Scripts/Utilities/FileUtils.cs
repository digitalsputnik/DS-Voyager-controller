using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Crosstales.FB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                if (to == string.Empty) return false;
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

        public static async Task<string> ReadAllTextAsync(string path)
        {
            string result;
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
                result = await reader.ReadToEndAsync();
            return result;
        }

        public static bool IsJsonValid(string strInput)
        {
            var comp = StringComparison.Ordinal;
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{", comp) && strInput.EndsWith("}", comp)) ||
                (strInput.StartsWith("[", comp) && strInput.EndsWith("]", comp)))
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch { return false; }
            }

            return false;
        }
    }

    public delegate void PathHandler(string path);
}
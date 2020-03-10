using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Crosstales.FB;
using Newtonsoft.Json.Linq;
using UnityEngine;
using VoyagerApp.UI;
using VoyagerApp.UI.Overlays;

namespace VoyagerApp.Utilities
{
    public static class FileUtils
    {
        public static string WorkspaceSavesPath
        {
            get => Path.Combine(Application.persistentDataPath, "workspaces");
        }

        public static string TempPath
        {
            get
            {
                var path = Path.Combine(Application.persistentDataPath, "temp");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                return path;
            }
        }

        public static void LoadVideoFromDevice(PathHandler onLoaded)
        {
            if (Application.isMobilePlatform)
            {
                NativeGallery.GetVideoFromGallery((string path) =>
                {
                    if (path == string.Empty || path == null)
                    {
                        onLoaded?.Invoke(null);
                        return;
                    }

                    if (Application.platform == RuntimePlatform.IPhonePlayer)
                    {
                        string name = "video_" + Guid.NewGuid().ToString().Substring(0, 4);

                        InputFieldMenu.Show("PICK NAME FOR VIDEO", name,
                            (string text) =>
                            {
                                name = text;

                                string newPath = Path.Combine(TempPath, name + ".MOV");
                                try
                                {
                                    Copy(path, newPath);
                                    onLoaded?.Invoke(newPath);
                                    return;
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError(ex);
                                }
                            }, 3, false);
                    }
                    else
                    {
                        onLoaded.Invoke(path);
                    }
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
                if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    DialogBox.Show(
                        "WARNING",
                        "Photos captured with iOS camera might not load.",
                        new string[] { "CANCEL", "OK" },
                        new Action[] {
                            () =>
                            {
                                onLoaded?.Invoke(null);
                            },
                            () =>
                            {
                                NativeGallery.GetImageFromGallery((string path) =>
                                {
                                    onLoaded?.Invoke(string.IsNullOrEmpty(path) ? null : path);
                                }, "", "image/*");
                            }
                        }
                    );
                }
                else
                {
                    NativeGallery.GetImageFromGallery((string path) =>
                    {
                        onLoaded.Invoke(path == "" ? null : path);
                    }, "", "image/*");
                }
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
            string result = null;
            try
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                    result = await reader.ReadToEndAsync();
            }
            finally
            {
                if (result == null)
                    result = "";
            }
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

        public static void Copy(string inputFilePath, string outputFilePath)
        {
            int bufferSize = 1024 * 1024;

            using (FileStream writeStream = new FileStream(outputFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
            {
                using (FileStream readStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read))
                {
                    writeStream.SetLength(readStream.Length);
                    int bytesRead = -1;
                    byte[] bytes = new byte[bufferSize];

                    while ((bytesRead = readStream.Read(bytes, 0, bufferSize)) > 0)
                        writeStream.Write(bytes, 0, bytesRead);
                }
            }
        }

        public static void PickFile(Action<string> onPick)
        {
            if (Application.isMobilePlatform)
            {
                // TODO: Implement!
            }
            else
            {
                string documents = DocumentsPath;
                ExtensionFilter[] extensions = { new ExtensionFilter("File") };
                string path = FileBrowser.OpenSingleFile("Open File", documents, extensions);
                onPick?.Invoke(path == "" ? null : path);
            }
        }

        public static void ClearCache()
        {
            foreach (string file in Directory.GetFiles(TempPath))
                File.Delete(file);
        }
    }

    public delegate void PathHandler(string path);
}
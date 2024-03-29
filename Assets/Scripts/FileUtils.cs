using Crosstales.FB;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VoyagerController.UI;

namespace VoyagerController
{
    public static class FileUtils
    {
        public static string TempPath
        {
            get
            {
                var path = Path.Combine(Application.persistentDataPath, "temp");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                return path;
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

        public static void LoadVideoFromDevice(PathHandler onLoaded)
        {
            if (Application.isMobilePlatform && !Application.isEditor)
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    if (NativeFilePicker.IsFilePickerBusy())
                        return;

                    var fileTypes = new[] { "video/*" };

                    NativeFilePicker.PickFile(path =>
                    {
                        if (string.IsNullOrEmpty(path))
                        {
                            onLoaded?.Invoke(null);
                            return;
                        }

                        var name = "video_" + Guid.NewGuid().ToString().Substring(0, 4);

                        InputFieldMenu.Show("PICK NAME FOR VIDEO", name,
                            text =>
                            {
                                name = text;
                                var extension = ".mp4";
                                var newPath = Path.Combine(TempPath, name + extension);

                                try
                                {
                                    Copy(path, newPath);
                                    onLoaded?.Invoke(newPath);
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError(ex);
                                }
                            }, 3, false);
                    }, fileTypes);   
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    NativeGallery.GetVideoFromGallery(path =>
                    {
                        if (string.IsNullOrEmpty(path))
                        {
                            onLoaded?.Invoke(null);
                            return;
                        }

                        var name = "video_" + Guid.NewGuid().ToString().Substring(0, 4);

                        InputFieldMenu.Show("PICK NAME FOR VIDEO", name,
                            text =>
                            {
                                name = text;
                                var extension = ".MOV";
                                var newPath = Path.Combine(TempPath, name + extension);

                                try
                                {
                                    Copy(path, newPath);
                                    onLoaded?.Invoke(newPath);
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError(ex);
                                }
                            }, 3, false);
                    });
                }
            }
            else
            {
                var documents = DocumentsPath;
                var extensions = new[] { new ExtensionFilter("Video", "mp4") };
                var path = FileBrowser.OpenSingleFile("Open Video", documents, extensions);
                onLoaded.Invoke(path == "" ? null : path);
            }
        }

        public static void LoadPictureFromDevice(PathHandler loaded, bool rename)
        {
            if (Application.isMobilePlatform && !Application.isEditor)
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    if (NativeFilePicker.IsFilePickerBusy())
                        return;

                    var fileTypes = new[] { "image/*" };

                    NativeFilePicker.PickFile(path =>
                    {
                        if (string.IsNullOrEmpty(path))
                        {
                            loaded?.Invoke(null);
                            return;
                        }

                        var name = "image_" + Guid.NewGuid().ToString().Substring(0, 4);

                        if (rename)
                        {
                            InputFieldMenu.Show("PICK NAME FOR IMAGE", name, text =>
                            {
                                name = text;
                                var newPath = Path.Combine(TempPath, name);
                                try
                                {
                                    Copy(path, newPath);
                                    loaded?.Invoke(newPath);
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError(ex);
                                }
                            }, 3, false);
                        }
                        else
                        {
                            loaded?.Invoke(path);
                        }
                    }, fileTypes);   
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    NativeGallery.GetImageFromGallery(path =>
                    {
                        if (string.IsNullOrEmpty(path))
                        {
                            loaded?.Invoke(null);
                            return;
                        }
                                        
                        var name = "image_" + Guid.NewGuid().ToString().Substring(0, 4);

                        if (rename)
                        {
                            InputFieldMenu.Show("PICK NAME FOR IMAGE", name, text =>
                            {
                                name = text;
                                var newPath = Path.Combine(TempPath, name);
                                try
                                {
                                    Copy(path, newPath);
                                    loaded?.Invoke(newPath);
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogError(ex);
                                }
                            }, 3, false);   
                        }
                        else
                        {
                            loaded?.Invoke(path);
                        }
                    });
                }
            }
            else
            {
                var documents = DocumentsPath;
                ExtensionFilter[] extensions =
                {
                    new ExtensionFilter("PNG Picture", "png"),
                    new ExtensionFilter("JPG Picture", "jpg"),
                    new ExtensionFilter("JPEG Picture", "jpeg")
                };
                var path = FileBrowser.OpenSingleFile("Open Picture", documents, extensions);
                loaded.Invoke(path == "" ? null : path);
            }
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
        
        public static void PickFile(Action<string> onPick)
        {
            if (Application.isMobilePlatform)
            {
                // TODO: Implement Native Filepicker!
            }
            else
            {
                var documents = DocumentsPath;
                ExtensionFilter[] extensions = { new ExtensionFilter("File") };
                var path = FileBrowser.OpenSingleFile("Open File", documents, extensions);
                onPick?.Invoke(path == "" ? null : path);
            }
        }

        public static bool SaveProject(string path, string name)
        {
            if (Application.isMobilePlatform)
            {
                bool success = false;

                if (NativeFilePicker.IsFilePickerBusy())
                    return false;

                NativeFilePicker.Permission permission = NativeFilePicker.ExportFile(path, (exportSuccessful) => 
                {
                    success = exportSuccessful;

                    // TODO: currently bool callback is always false, that should not be tha case

                    if (File.Exists(path))
                        File.Delete(path);
                });

                return success;
            }
            else
            {
                string documents = DocumentsPath;
                ExtensionFilter[] extensions = { new ExtensionFilter("Compressed Zip", "zip") };
                string to = FileBrowser.SaveFile("Save Project", documents, $"{name}", extensions);
                if (to == string.Empty) return false;
                File.Move(path, to);
                return true;
            }
        }

        public static void LoadProject(PathHandler onLoaded)
        {
            if (Application.isMobilePlatform)
            {
                if (NativeFilePicker.IsFilePickerBusy())
                    return;

#if UNITY_ANDROID
                // Use MIMEs on Android
                string[] fileTypes = new string[] { "application/zip" };
#else
                // Use UTIs on iOS
                string[] fileTypes = new string[] { "public.archive" };
#endif

                NativeFilePicker.Permission permission = NativeFilePicker.PickFile(path =>
                {
                    if (string.IsNullOrEmpty(path))
                        onLoaded?.Invoke(null);
                    else
                        onLoaded?.Invoke(path);
                }, fileTypes);
            }
            else
            {
                string documents = DocumentsPath;
                ExtensionFilter[] extensions = { new ExtensionFilter("Compressed Zip", "zip") };
                string path = FileBrowser.OpenSingleFile("Open Project", documents, extensions);
                onLoaded.Invoke(path == "" ? null : path);
            }
        }
    }

    public delegate void PathHandler(string path);
}
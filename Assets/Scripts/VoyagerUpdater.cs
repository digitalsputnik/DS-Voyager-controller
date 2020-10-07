using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using DigitalSputnik;
using DigitalSputnik.Voyager;
using UnityEngine;

//#if !UNITY_IOS
using Renci.SshNet;
//#endif

namespace VoyagerController
{
    public class VoyagerUpdater : MonoBehaviour
    {
        private static VoyagerUpdater _instance;
        private void Awake() => _instance = this;
        
        private const string LAMP_USERNAME = "root";
        private const string LAMP_PASSWORD = "controlground";

        private const string BUNDLE_DEST = "/mnt/data/update_temp";
        private const string BUNDLE_FILE = "voyager_update.tar";

        private const double INSTALLATION_TIMEOUT = 300;
        
        [SerializeField] private string _version = "";

        private readonly Dictionary<string, byte[]> assets = new Dictionary<string, byte[]>();
        private int _finishedCount = 0;

        private void Start()
        {
            PreloadAsset();
        }

        public static string Version => _instance._version;

        public static int UpdatesFinished => _instance._finishedCount;

        private void PreloadAsset()
        {
            var asset = Resources.Load($"Update/{BUNDLE_FILE}") as TextAsset;

            if (asset == null)
            {
                Debugger.LogError($"[VOYAGER UPDATE UTILITY] " + $"Could not preload file {BUNDLE_FILE}");
                return;
            }

            var data = asset.bytes;
            assets.Add(BUNDLE_FILE, data);
        }

        public static void UpdateLamp(VoyagerLamp lamp, VoyagerUpdateHandler onDone, VoyagerUpdateMessageHandler onMessage)
        {
            new Thread(() => _instance.UpdateLampThread(lamp, onDone, onMessage)).Start();
        }

        private void UpdateLampThread(VoyagerLamp lamp, VoyagerUpdateHandler onDone, VoyagerUpdateMessageHandler onMessage)
        {
//#if !UNITY_IOS
            var ssh = CreateSshClient(lamp);
            var sftp = CreateSftpClient(lamp);

            var success = false;
            var error = "";

            try
            {
				onMessage?.Invoke(new VoyagerUpdateMessage(lamp, "Starting update. Uploading update to lamp."));
				UploadUpdateToLamp(sftp);
				onMessage?.Invoke(new VoyagerUpdateMessage(lamp, "Installing update."));
				InstallUpdateOnLamp(ssh);
                onMessage?.Invoke(new VoyagerUpdateMessage(lamp, "Successfully installed new software on lamp."));
                success = true;
            }
            catch (Exception ex)
			{
                Debugger.LogError(ex.Message);
				onMessage?.Invoke(new VoyagerUpdateMessage(lamp, "Failed lamp update."));
				error = ex.Message;
			}
            finally
            {
                ssh.Dispose();
                sftp.Dispose();
            }

            onDone?.Invoke(new VoyagerUpdateResponse(success, error, lamp));
            _finishedCount++;
//#endif
        }

//#if !UNITY_IOS

        private static SshClient CreateSshClient(Lamp lamp)
        {
            var address = (lamp.Endpoint as LampNetworkEndPoint)?.address;
            return address != null ? new SshClient(address.ToString(), LAMP_USERNAME, LAMP_PASSWORD) : null;
        }

        private static SftpClient CreateSftpClient(Lamp lamp)
        {
            var address = (lamp.Endpoint as LampNetworkEndPoint)?.address;
            return address != null ? new SftpClient(address.ToString(), LAMP_USERNAME, LAMP_PASSWORD) : null;
        }

        private void UploadUpdateToLamp(SftpClient sftp)
        {
            sftp.Connect();

            SetDestination(sftp, BUNDLE_DEST);
            UploadByteAsset(sftp, BUNDLE_FILE);

            sftp.Disconnect();
        }

        private static void InstallUpdateOnLamp(SshClient ssh)
        {
            ssh.Connect();

            const string DOS2_UNIX_CMD = "dos2unix " + 
                                         BUNDLE_DEST + "/*.py " + 
                                         BUNDLE_DEST + "/*.sh " + 
                                         BUNDLE_DEST + "/*.txt " + 
                                         BUNDLE_DEST + "/*.chk ";
            
            ssh.RunCommand(DOS2_UNIX_CMD);

            ssh.RunCommand($"tar -xf {BUNDLE_DEST}/{BUNDLE_FILE} -C /mnt/data/");

            var installationCmd = $"setsid python3 {BUNDLE_DEST}/update3.py &";
            var installation = ssh.CreateCommand(installationCmd);
            installation.BeginExecute();
            
            var done = false;
            var success = false;

            var startTime = TimeUtils.Epoch;

            using (var reader = new StreamReader(installation.OutputStream, Encoding.UTF8, true, 1024))
            {
                var passed = TimeUtils.Epoch - startTime;
                while (!done && passed < INSTALLATION_TIMEOUT)
                {
                    var output = reader.ReadLine();
                    
                    if (output == null) continue;

                    success = !output.Contains("FAIL");
                    done = output.Contains("REBOOT") || output.Contains("UPDATE SUCCESSFUL") || output.Contains("FAIL");
                }
            }

            if (!success) throw new Exception("Update failed!");
            ssh.Disconnect();
        }

        private static void SetDestination(SftpClient sftp, string directory)
        {
            if (!sftp.Exists(directory))
                sftp.CreateDirectory(directory);
            sftp.ChangeDirectory(directory);
        }

        private void UploadByteAsset(SftpClient sftp, string asset)
        {
            var data = assets[asset];
            var destination = Path.GetFileName(asset);

            using (Stream stream = new MemoryStream(data))
            {
                sftp.BufferSize = 4 * 1024;
                sftp.UploadFile(stream, destination);
            }
        }
//#endif
    }

    public delegate void VoyagerUpdateHandler(VoyagerUpdateResponse response);
	public delegate void VoyagerUpdateMessageHandler(VoyagerUpdateMessage message);

    public readonly struct VoyagerUpdateResponse
    {
        public bool Success { get; }
        public string Error { get; }
        public VoyagerLamp Lamp { get; }

        public VoyagerUpdateResponse(bool success, string error, VoyagerLamp lamp)
        {
            Success = success;
            Error = error;
            Lamp = lamp;
        }
    }

    public readonly struct VoyagerUpdateMessage
    {
        public VoyagerLamp Lamp { get; }
        public string Message { get; }

        public VoyagerUpdateMessage(VoyagerLamp lamp, string message)
        {
            Lamp = lamp;
            Message = message;
        }
    }
}
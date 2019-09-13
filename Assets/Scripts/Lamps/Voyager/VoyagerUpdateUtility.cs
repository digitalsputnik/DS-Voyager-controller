using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;
using VoyagerApp.Utilities;

//#if !UNITY_IOS
using Renci.SshNet;
//#endif

namespace VoyagerApp.Lamps.Voyager
{
    public class VoyagerUpdateUtility
    {
        const string LAMP_USERNAME = "root";
        const string LAMP_PASSWORD = "controlground";

        const string BUNDLE_DEST = "/mnt/data/update_temp";
        const string BUNDLE_FILE = "voyager_update.tar";

        const double INSTALLATION_TIMEOUT = 300;

        Dictionary<string, byte[]> assets = new Dictionary<string, byte[]>();

        public VoyagerUpdateUtility()
        {
            PreloadAsset(BUNDLE_FILE);
        }

        public void PreloadAsset(string name)
        {
            TextAsset asset = Resources.Load($"Update/{name}") as TextAsset;

            if (asset == null)
            {
                Debug.LogError($"[VOYAGER UPDATE UTILITY] " +
                               $"Could not preload file {name}");
                return;
            }

            byte[] data = asset.bytes;
            assets.Add(name, data);
        }

        public void UpdateLamp(VoyagerLamp lamp,
                               VoyagerUpdateHandler onDone,
                               VoyagerUpdateMessageHandler onMessage)
        {
            new Thread(() => UpdateLampThread(lamp, onDone, onMessage)).Start();
        }

		void UpdateLampThread(VoyagerLamp lamp,
                              VoyagerUpdateHandler onDone,
                              VoyagerUpdateMessageHandler onMessage)
        {
//#if !UNITY_IOS
			SshClient ssh = CreateSshClient(lamp);
            SftpClient sftp = CreateSftpClient(lamp);

            bool success = false;
            string error = "";

            try
            {
				onMessage?.Invoke(new VoyagerUpdateMessage(lamp, "Starting update. Uploading update to lamp."));
				UploadUpdateToLamp(sftp);
				onMessage?.Invoke(new VoyagerUpdateMessage(lamp, "Installing update."));
				InstallUpdateOnLamp(ssh, out bool reboot);
				onMessage?.Invoke(new VoyagerUpdateMessage(lamp, "Checking if install was successful."));
				CheckVersionOnLamp(ssh);
				onMessage?.Invoke(new VoyagerUpdateMessage(lamp, "Successfully installed new software on lamp."));
                success = true;
            }
            catch (Exception ex)
			{
				onMessage?.Invoke(new VoyagerUpdateMessage(lamp, "Failed lamp update."));
				error = ex.Message;
			}
            finally
            {
                ssh.Dispose();
                sftp.Dispose();
            }

            onDone?.Invoke(new VoyagerUpdateResponse(success, error, lamp));
//#endif
        }

//#if !UNITY_IOS

        SshClient CreateSshClient(VoyagerLamp lamp)
        {
            return new SshClient(lamp.address.ToString(),
                                 LAMP_USERNAME,
                                 LAMP_PASSWORD);
        }

        SftpClient CreateSftpClient(VoyagerLamp lamp)
        {
            return new SftpClient(lamp.address.ToString(),
                                  LAMP_USERNAME,
                                  LAMP_PASSWORD);
        }

        void UploadUpdateToLamp(SftpClient sftp)
        {
            sftp.Connect();

            SetDestination(sftp, BUNDLE_DEST);
            UploadByteAsset(sftp, BUNDLE_FILE);

            sftp.Disconnect();
        }

        void InstallUpdateOnLamp(SshClient ssh, out bool reboot)
        {
            ssh.Connect();
            reboot = false;

            var dos2unixCmd = "dos2unix " +
                  BUNDLE_DEST + "/*.py " +
                  BUNDLE_DEST + "/*.sh " +
                  BUNDLE_DEST + "/*.txt " +
                  BUNDLE_DEST + "/*.chk ";
            ssh.RunCommand(dos2unixCmd);

            ssh.RunCommand($"tar -xf {BUNDLE_DEST}/{BUNDLE_FILE} -C /mnt/data/");

            var installationCmd = $"python3 {BUNDLE_DEST}/update3.py";
            var installation = ssh.CreateCommand(installationCmd);
            installation.BeginExecute();

            bool done = false;
            bool success = false;

            double startTime = TimeUtils.Epoch;

            using (var reader = new StreamReader(installation.OutputStream,
                                                 Encoding.UTF8, true, 1024))
            {
                double passed = TimeUtils.Epoch - startTime;
                string output;
                while (!done && passed < INSTALLATION_TIMEOUT)
                {
                    output = reader.ReadLine();
                    if (output == null) continue;

                    success = !output.Contains("FAIL");
                    reboot = output.Contains("REBOOT");
                    done = output.Contains("REBOOT") ||
                               output.Contains("UPDATE SUCCESSFUL") ||
                               output.Contains("FAIL");
                }
            }

            if (!success)
                throw new Exception("Update failed!");

            ssh.Disconnect();
        }

        void CheckVersionOnLamp(SshClient ssh)
        {
            ssh.Connect();

            ssh.RunCommand("dos2unix /mnt/data/animation/*");

            var testCmds = new string[]
            {
                    "ls -l /mnt/data/animation/AnimationPlayer.py | awk '{print $5}'",
                    "ls -l /mnt/data/animation/PythonReceiver.py | awk '{print $5}'",
                    "ls -l /mnt/data/animation/HueCalibration.csv | awk '{print $5}'",
                    "ls -l /mnt/data/animation/IntensityCalibration.csv | awk '{print $5}'",
                    "ls -l /mnt/data/animation/TemperatureCalibration.csv | awk '{print $5}'"
            };

            foreach (var testCmd in testCmds)
            {
                SshCommand status = ssh.RunCommand(testCmd);
                if (Convert.ToInt32(status.Result) == 0)
                    throw new Exception("Animation and autorun failed!");
            }

            ssh.Disconnect();
        }

        void SetDestination(SftpClient sftp, string directory)
        {
            if (!sftp.Exists(directory))
                sftp.CreateDirectory(directory);
            sftp.ChangeDirectory(directory);
        }

        void UploadByteAsset(SftpClient sftp, string asset)
        {
            byte[] data = assets[asset];
            string destination = Path.GetFileName(asset);

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

    public struct VoyagerUpdateResponse
    {
        public bool success;
        public string error;
        public VoyagerLamp lamp;

        public VoyagerUpdateResponse(bool success, string error, VoyagerLamp lamp)
        {
            this.success = success;
            this.error = error;
            this.lamp = lamp;
        }
    }

    public struct VoyagerUpdateMessage
    {
        public VoyagerLamp lamp;
        public string message;

        public VoyagerUpdateMessage(VoyagerLamp lamp, string message)
        {
            this.lamp = lamp;
            this.message = message;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using Renci.SshNet;
using System.IO;
using System.Text;
using Voyager.Lamps;
using System.Net;

public class LampUpdater : MonoBehaviour {

	LampManager lampManager;

	Queue<string> UpdateableLamps = new Queue<string>(); // As IPs
	Dictionary<string, Thread> UpdateThreads = new Dictionary<string, Thread>();
	public Dictionary<string, float> UpdateProgress = new Dictionary<string, float>();
	Dictionary<string, string> FailedToUpdate = new Dictionary<string, string>();

    List<string> UpdatedLamps = new List<string>();
 
	Dictionary<string, string> TextAssets = new Dictionary<string, string>();
	Dictionary<string, byte[]> ByteAssets = new Dictionary<string, byte[]>();

	public bool Updating { get; private set; }
	public int UpdatesInProgress { get; private set; }

	public const string LampUsername = "root";
	public const string LampPassword = "groundcontrol";

	const string NumpyDestinationFolder = "/media/numpy_install_temp";
	const string NetworkFolder = "/media/network_control.status";
	const string AnimationDestinationFolder = "/media/animation";
	const string AnimationDestinationFolder3 = "/mnt/data/animation";
	const string AutorunDestinationFolder = "/media";
	const string AutorunDestinationFolder3 = "/mnt/data";
	const string Rev3InstallationDirectory = "/mnt/data/update_temp";

	const string UDPfile = "ut2.5.py";
    const string UDPfile3 = "ut2.6.py";
    const string AutorunSourceFile = "autorun.sh";
    const string AutorunSourceFile3 = "autorun3.sh";
    const string NetworkFile = "autoconnect.sh";

	string[] NumpyInstallationFiles =
    {
        "libblas3_1.2.20110419-10_armhf.deb",
        "libblas-common_1.2.20110419-10_armhf.deb",
        "libgfortran3_4.9.2-10_armhf.deb",
        "liblapack3_3.5.0-4_armhf.deb",
        "python3-numpy_1.8.2-2_armhf.deb"
    };

	string[] AnimationInstallationFiles =
    {
        "PythonReceiver.py",
        "AnimationPlayer.py",
        "HueCalibration.csv",
        "IntensityCalibration.csv",
        "TemperatureCalibration.csv",
        "version"
    };
    
    string[] BundleInstallationFiles =
    {
		/*
		"ap_only.py",
        "autoconnect.sh",
        "aux_disable_shutdown_30_sec.py",
        "checklist.chk",
        "client_only.py",
        "connect_cm.sh",
        "connect_cm_v2.sh",
        "lpc_firmware_exit_from_bootloader_hw3.py",
        "lpc_firmware_update_hw3.sh",
        "lpc_firmware_version.py",
        "netchanger.sh",
        //"network_mode",
		"rene-timesync.tar.xz",
        "serial_check_1.sh",
        "serial_check_2.sh",
        "ssidlist.txt",
        "timecompare.py",
        "timesync_router.py",
		"timesync_service.py",
        "timesync-ask.py",
        "update1.sh",
        "update3.py",
        "ut2.6.py",
        "voyager_lpc_release_user_update.bin"
		*/
		"HW3_voyager_update.tar"
    };

	void Start()
	{
		PreloadFiles();
		lampManager = GameObject.FindWithTag("LampManager").GetComponent<LampManager>();
	}

    void PreloadFiles()
	{
		foreach (string filename in NumpyInstallationFiles)
            LoadAssets(filename);
        foreach (string filename in AnimationInstallationFiles)
            LoadAssets(filename);
        foreach (string filename in BundleInstallationFiles)
            LoadAssets(filename);

        LoadAssets(UDPfile);
        LoadAssets(UDPfile3);
        LoadAssets(AutorunSourceFile);
        LoadAssets(AutorunSourceFile3);
        LoadAssets(NetworkFile);
	}

    void LoadAssets(string filename)
	{
		if(!TextAssets.ContainsKey(filename) && !ByteAssets.ContainsKey(filename))
		{
			//Debug.Log(filename);
			TextAsset resource = Resources.Load(filename) as TextAsset;
			if (resource.text.Length < resource.bytes.Length)
				ByteAssets.Add(filename, resource.bytes);
			else
				TextAssets.Add(filename, resource.text);
		}
	}

	public void UpdateLampsSoftware(List<string> lampIPs)
	{
		foreach(string lampIP in lampIPs)
		{
			if(!UpdateableLamps.Contains(lampIP) && !UpdatedLamps.Contains(lampIP) && !UpdateThreads.ContainsKey(lampIP))
				UpdateableLamps.Enqueue(lampIP);
		}
	}

	void FixedUpdate()
	{
		while (UpdateableLamps.Count > 0)
		{
			if (!Updating) UpdateStarted();

			string lampIP = UpdateableLamps.Dequeue();

			if (!UpdateThreads.ContainsKey(lampIP))
			{
				ThreadStart updateStart = new ThreadStart(delegate { LampUpdateThread(lampIP); });
				Thread updateThread = new Thread(updateStart);
				UpdateThreads.Add(lampIP, updateThread);
				updateThread.Start();
			}
		}

		if (Updating && UpdateableLamps.Count == 0 && UpdateThreads.Count == 0)
			UpdateFinished();
	}

    void UpdateStarted()
	{
		Updating = true;
	}

    void UpdateFinished()
	{
		Updating = false;
		UpdatesInProgress = 0;

		if(FailedToUpdate.Count > 0)
		{
			string errors = "\n";
			foreach (var error in FailedToUpdate.Values)
				errors += error + "\n";

			DialogBoxSettings settings = new DialogBoxSettings();
			settings.ShowIgnoreBtn = false;
			settings.Info = errors;
			settings.RespondBtnText = "Retry";
			settings.Title = "Failed to update " + FailedToUpdate.Count + " lamp(s)";
			settings.Type = DialogBoxType.Error;

			DialogBox.OnUserCallback += RetryPressed;
			DialogBox.Show(settings);
		}
		else
		    DialogBox.ShowInfo("Lamps updated!");      
	}

	void RetryPressed(DialogBoxCallbackEventArgs e)
	{
		UpdateLampsSoftware(new List<string>(FailedToUpdate.Keys));
		FailedToUpdate.Clear();
		DialogBox.OnUserCallback -= RetryPressed;
	}

    void UpdateFailed(string lampIP, string error)
	{
		lampManager.GetLamp(IPAddress.Parse(lampIP)).updatingFirmware = false;
		if (!FailedToUpdate.ContainsKey(lampIP))
			FailedToUpdate.Add(lampIP, error);
	}

	void LampUpdateThread(string lampIP)
    {
		Lamp lamp = lampManager.GetLamp(IPAddress.Parse(lampIP));
		lamp.updatingFirmware = true;
  
		UpdatesInProgress++;
        lock (UpdateProgress) { UpdateProgress.Add(lampIP, 0.0f); }

        int updateSteps = 4;
        float progressStep = 1.0f / updateSteps;

        bool numpyNotInstalled = false;
        bool isRev3 = false;
        bool rebootNeeded = false;


        SshClient ssh = new SshClient(lampIP, LampUsername, LampPassword);
        SftpClient sftp = new SftpClient(lampIP, LampUsername, LampPassword);

        for (int i = 0; i < updateSteps; i++)
        {
            bool failed = false;

            switch(i)
            {
                case 0:
                    failed = !GetLampStatus(lampIP, ssh, out numpyNotInstalled, out isRev3);
                    break;
                case 1:
                    failed = !UploadFiles(lampIP, sftp, numpyNotInstalled, isRev3);
                    break;
                case 2:
                    failed = !UpdateLamp(lampIP, ssh, numpyNotInstalled, isRev3, out rebootNeeded);
                    break;
                case 3:
                    failed = !VersionControll(lampIP, ssh, isRev3);
                    break;
            }

            if (failed)
                break;
            
            lock (UpdateProgress) { UpdateProgress[lampIP] += progressStep; }
        }
        
        ssh.Dispose();
        sftp.Dispose();
		lamp.updatingFirmware = false;
		UpdateThreads.Remove(lampIP);
		UpdatedLamps.Add(lampIP);
	}

	bool GetLampStatus (string lampIP, SshClient ssh, out bool numpyNotInstalled, out bool isRev3)
	{
		isRev3 = false;
		numpyNotInstalled = false;

		try
        {
			ssh.Connect();

            SshCommand testRevisionCmd = ssh.RunCommand("uname -r");
            isRev3 = testRevisionCmd.Result == "4.4.30-pro\n" ? false : true;
			testRevisionCmd.Dispose();

            if (!isRev3)
            {
                SshCommand testStatusCmd = ssh.RunCommand("dpkg -s python3-numpy | grep Status");
                numpyNotInstalled = testStatusCmd.Result == "Status: install ok installed\n" ? false : true;
				testStatusCmd.Dispose();
            }

            ssh.Disconnect();
			return true;
        }
        catch(Exception ex)
        {
			Debug.LogError(ex.Message);
			ssh.Disconnect();
			UpdateFailed(lampIP, ex.Message);
			return false;
        }
	}

	bool UploadFiles(string lampIP, SftpClient sftp, bool numpyNotInstalled, bool isRev3)
	{
		try
		{
			sftp.Connect();

            if (numpyNotInstalled)
            {
                if (!sftp.Exists(NumpyDestinationFolder))
                    sftp.CreateDirectory(NumpyDestinationFolder);

                sftp.ChangeDirectory(NumpyDestinationFolder);
                foreach (string filename in NumpyInstallationFiles)
                    UploadFileAsset(filename, sftp);
            }

            if (!isRev3)
            {
                sftp.ChangeDirectory(NetworkFolder);
                UploadFileAsset(NetworkFile, sftp);
            }

            string animDestFolder = isRev3 ? AnimationDestinationFolder3 : AnimationDestinationFolder;

            if (!sftp.Exists(animDestFolder))
                sftp.CreateDirectory(animDestFolder);

            sftp.ChangeDirectory(animDestFolder);
            foreach (string filename in AnimationInstallationFiles)
                UploadFileAsset(filename, sftp);

            if (isRev3)
            {
                if (!sftp.Exists(Rev3InstallationDirectory))
                    sftp.CreateDirectory(Rev3InstallationDirectory);

                sftp.ChangeDirectory(Rev3InstallationDirectory);
                foreach (string filename in BundleInstallationFiles)
                    UploadFileAsset(filename, sftp);
            }
            else
            {
                string udp = isRev3 ? UDPfile3 : UDPfile;
                sftp.ChangeDirectory(AutorunDestinationFolder);
                UploadFileAsset(UDPfile, sftp);
            }

            string autorunDestFolder = isRev3 ? Rev3InstallationDirectory : AutorunDestinationFolder;
            string autorunFile = isRev3 ? AutorunSourceFile3 : AutorunSourceFile;

            sftp.ChangeDirectory(autorunDestFolder);
            UploadFileAsset(autorunFile, sftp, "autorun.sh");

            sftp.Disconnect();
			return true;
        }
        catch (Exception ex)
        {
			Debug.LogError(ex.Message + " || " + ex.StackTrace);
			UpdateFailed(lampIP, ex.Message);
			sftp.Disconnect();
			return false;
        }
	}

	bool UpdateLamp(string lampIP, SshClient ssh, bool numpyNotInstalled, bool isRev3, out bool rebootNeeded)
	{
		rebootNeeded = false;

		try
        {
			ssh.Connect();

            if (numpyNotInstalled)
            {
                ssh.RunCommand("mount -o remount,rw /dev/ubi0_0 / -t ubifs");
                ssh.RunCommand("dpkg -i /media/numpy_install_temp/*");
                SshCommand testStatusCmd = ssh.RunCommand("dpkg -s python3-numpy | grep Status");

                if (testStatusCmd.Result != "Status: install ok installed\n")
                {
                    ssh.Disconnect();
					testStatusCmd.Dispose();
                    throw new Exception("Numpy failed!");
                }

				testStatusCmd.Dispose();
                ssh.RunCommand("rm -r /media/numpy_install_temp");
            }

            if (isRev3)
            {
				string dos2unixCmd = "dos2unix " + Rev3InstallationDirectory + "/*.py " + Rev3InstallationDirectory + "/*.sh " + Rev3InstallationDirectory + "/*.txt "+ Rev3InstallationDirectory + "/*.chk";
                SshCommand transformResult = ssh.RunCommand(dos2unixCmd);
				SshCommand unpackResult = ssh.RunCommand("tar -xf " + Rev3InstallationDirectory + "/HW3_voyager_update.tar -C /mnt/data/");
                SshCommand installationCmd = ssh.CreateCommand("python3 " + Rev3InstallationDirectory + "/update3.py");
                IAsyncResult installationScriptResult = installationCmd.BeginExecute();

                bool installationDone = false;
                bool installationSuccess = false;

                using (StreamReader reader = new StreamReader(installationCmd.OutputStream, Encoding.UTF8, true, 1024))
                {
                    string output = null;
                    while (!installationDone)
                    {
                        output = reader.ReadLine();
                        Debug.Log(output);
                        if (output == null)
                            continue;

                        if (output.Contains("UPDATE SUCCESSFUL"))
                        {
                            installationSuccess = true;
                            installationDone = true;
                        }

                        if (output.Contains("REBOOT"))
                        {
                            rebootNeeded = true;
                            installationDone = true;
                        }

                        if (output.Contains("FAIL"))
                            installationDone = true;

                    }
                }

				transformResult.Dispose();
				installationCmd.Dispose();

                if (!installationSuccess)
                {
                    ssh.Disconnect();
                    throw new Exception("Update for rev3 failed!");
                }
            }
            ssh.Disconnect();
            return true;
        }
        catch (Exception ex)
        {
			Debug.LogError(ex.Message);
			ssh.Disconnect();
			UpdateFailed(lampIP, ex.Message);
            return false;
        }
	}

	bool VersionControll(string lampIP, SshClient ssh, bool isRev3)
	{
		try
        {
			ssh.Connect();
            string[] testCmdStrings = null;

            ssh.RunCommand("dos2unix /mnt/data/animation/*");

            if (isRev3)
            {
                testCmdStrings = new string[]
                {
                        "ls -l /mnt/data/animation/AnimationPlayer.py | awk '{print $5}'",
                        "ls -l /mnt/data/animation/PythonReceiver.py | awk '{print $5}'",
                        "ls -l /mnt/data/animation/HueCalibration.csv | awk '{print $5}'",
                        "ls -l /mnt/data/animation/IntensityCalibration.csv | awk '{print $5}'",
                        "ls -l /mnt/data/animation/TemperatureCalibration.csv | awk '{print $5}'"
                };
            }
            else
            {
                testCmdStrings = new string[]
                {
                        "ls -l /media/animation/AnimationPlayer.py | awk '{print $5}'",
                        "ls -l /media/animation/PythonReceiver.py | awk '{print $5}'",
                        "ls -l /media/autorun.sh | awk '{print $5}'", "ls -l /media/ut2.5.py | awk '{print $5}'"
                };
            }

            foreach (var testCmd in testCmdStrings)
            {
                SshCommand testStatusCmd = ssh.RunCommand(testCmd);
                if (!(Convert.ToInt32(testStatusCmd.Result) > 0))
                {
                    ssh.Disconnect();
					testStatusCmd.Dispose();
                    throw new Exception("Animation and autorun failed!");
                }
				testStatusCmd.Dispose();
            }

            ssh.Disconnect();
			return true;
        }
        catch (Exception ex)
        {
			Debug.LogError(ex.Message);
			ssh.Disconnect();
			UpdateFailed(lampIP, ex.Message);
			return false;
        }
	}
    
	void UploadFileAsset(string sourceFile, SftpClient sftp, string DestinationFileName = "")
    {
		string destFilename = DestinationFileName == "" ? Path.GetFileName(sourceFile) : DestinationFileName;

		if(TextAssets.ContainsKey(sourceFile))
		{
			string resource = TextAssets[sourceFile];

			MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);

			writer.Write(resource);
            writer.Flush();
            stream.Position = 0;

			sftp.BufferSize = 4 * 1024;
			sftp.UploadFile(stream, destFilename);

			writer.Dispose();
			stream.Dispose();
		}
		else if (ByteAssets.ContainsKey(sourceFile))
		{
			byte[] resource = ByteAssets[sourceFile];
			Debug.Log (sourceFile);
			Debug.Log (resource.Length);
            using (Stream s = new MemoryStream(resource))
            {
                sftp.BufferSize = 4 * 1024;
                sftp.UploadFile(s, destFilename);
            }
		}
    }
}
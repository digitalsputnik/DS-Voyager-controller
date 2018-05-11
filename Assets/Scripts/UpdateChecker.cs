using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Threading;
using Renci.SshNet;

public class UpdateChecker : MonoBehaviour {

    public GameObject Menu;
    public GameObject LampList;
    public Button OkCancelButton;
    public Button RetryButton;
    public Slider ProgressBar;
    public Text UpdateText;

    public Text ErrorText;

    private float ProgressBarValue = 0;
    private string OkCancelButtonText = "";
    private string RetryButtonText = "";
    private string UpdateTextValue = "";

    public string LampUsername = "root";
    public string LampPassword = "groundcontrol";
    public string LatestVersion = "0.5";

    private List<string> lampsToBeUpdated;
    private int UpdateCount;

    string NumpyDestinationFolder = "/media/numpy_install_temp";
    string NetworkFolder = "/media/network_control.status";
    string AnimationDestinationFolder = "/media/animation";
    string AnimationDestinationFolder3 = "/mnt/data/animation";
    string AutorunDestinationFolder = "/media";
    string AutorunDestinationFolder3 = "/mnt/data";
    string Rev3InstallationDirectory = "/mnt/data/update_temp";

    //Numpy installation files on Resources
    private string[] NumpyInstallationFiles = new string[]
    {
        "libblas3_1.2.20110419-10_armhf.deb",
        "libblas-common_1.2.20110419-10_armhf.deb",
        "libgfortran3_4.9.2-10_armhf.deb",
        "liblapack3_3.5.0-4_armhf.deb",
        "python3-numpy_1.8.2-2_armhf.deb"
    };

    //Animation files
    private string[] AnimationInstallationFiles = new string[]
    {
        "PythonReceiver.py",
        "AnimationPlayer.py",
        "HueCalibration.csv",
        "IntensityCalibration.csv",
        "TemperatureCalibration.csv",
        "version"
    };

    //Bundle files
    private string[] BundleInstallationFiles = new string[]
    {
        "aux_disable_shutdown_30_sec.py",
        "checklist.chk",
        "connect_cm.sh",
        "lpc_firmware_exit_from_bootloader_hw3.py",
        "lpc_firmware_update_hw3.sh",
        "lpc_firmware_version.py",
        "netchanger.sh",
        "serial_check_1.sh",
        "serial_check_2.sh",
        "ssidlist.txt",
        "update1.sh",
        "update3.py",
        "ut2.6.py",
        "voyager_lpc_release_user_update.bin",
        "autoconnect.sh",
        "timecompare.py",
        "timesync-ask.py"
    };

    //utFile
    private string UDPfile = "ut2.5.py";
    private string UDPfile3 = "ut2.6.py";

    //Autorun file
    private string AutorunSourceFile = "autorun.sh";
    private string AutorunSourceFile3 = "autorun3.sh";

    //Network file for rev2
    private string NetworkFile = "autoconnect.sh";

    // Use this for initialization
    void Start () {
        OkCancelButton.onClick.AddListener(OnOkButtinClick);
        RetryButton.onClick.AddListener(OnRetryButtonClick);
        //StartCoroutine("Updater");
        //LampsUpdate();
    }

    private void OnRetryButtonClick()
    {
        RetryButtonText = "";
        RetryButton.GetComponentInChildren<Text>().text = RetryButtonText;
        OkCancelButtonText = "";
        OkCancelButton.GetComponentInChildren<Text>().text = OkCancelButtonText;
        //Retry to update lamps which were not successfully updated
        StartCoroutine("LampsUpdate");
        //throw new NotImplementedException();
    }

    private void OnOkButtinClick()
    {
        OkCancelButtonText = "";
        OkCancelButton.GetComponentInChildren<Text>().text = OkCancelButtonText;
        RetryButtonText = "";
        RetryButton.GetComponentInChildren<Text>().text = RetryButtonText;
        SetActive(false);
    }

    // Update is called once per frame
    void Update () {
        //Set loading values and buttons
        if (this.gameObject.activeSelf)
        {
            ProgressBar.value = ProgressBarValue;
            UpdateText.text = UpdateTextValue;
            if (OkCancelButtonText != "")
            {
                OkCancelButton.GetComponentInChildren<Text>().text = OkCancelButtonText;
                OkCancelButton.gameObject.SetActive(true);
            }
            else
            {
                OkCancelButton.gameObject.SetActive(false);
            }

            if (RetryButtonText != "")
            {
                RetryButton.GetComponentInChildren<Text>().text = RetryButtonText;
                RetryButton.gameObject.SetActive(true);
            }
            else
            {
                RetryButton.gameObject.SetActive(false);
            }

        }
    }

    public void SetActive(bool active)
    {
        //Turn this gameobject active and turn off menu!
        this.gameObject.SetActive(active);
        Menu.SetActive(!active);
        LampList.SetActive(false);
        OkCancelButton.gameObject.SetActive(false);
        RetryButton.gameObject.SetActive(false);

        if (active)
        {
            //Start updating coroutine!
            //readThread = new Thread(new ThreadStart(GetLampColorData));
            //updateThread = new Thread(new ThreadStart(LampsUpdate));//new ThreadStart(GetLampColorData));
            //updateThread.IsBackground = true;
            //updateThread.Start();
            //updateCoroutine = LampsUpdate(lampsToBeUpdated);
            //StartCoroutine("LampsUpdate", lampsToBeUpdated);

            //StartCoroutine("Updater");
            StartCoroutine("LampsUpdate");
        }

    }

    public void UpdateLampsSoftware(List<string> LampIPs)
    {
        //Check for lamp software
        List<string> LampsToBeUpdated = LampIPs;//GetUpdateableLamps(LampIPs);

        if (LampsToBeUpdated.Count > 0)
        {
            lampsToBeUpdated = LampsToBeUpdated;
            UpdateCount = lampsToBeUpdated.Count;

            UpdateTextValue = string.Format("Updating lamps {0}/{1}, please wait...", 1, UpdateCount);
            SetActive(true);
        }
    }

    //TODO: Remove!
    public List<string> GetUpdateableLamps(List<string> LampIPs)
    {
        List<string> LampsToBeUpdated = new List<string>();

        try
        {
            foreach (var lampIP in LampIPs)
            {
                bool[] NewSoftware = { false, false };
                using (SshClient sshClient = new SshClient(lampIP, LampUsername, LampPassword))
                {
                    sshClient.Connect();
                    var TestStatusCommand = sshClient.RunCommand("dpkg -s python3-numpy | grep Status");
                    NewSoftware[0] = TestStatusCommand.Result == "Status: install ok installed\n" ? true : false;
                    TestStatusCommand = sshClient.RunCommand("cat /media/animation/version");
                    NewSoftware[1] = TestStatusCommand.Result == LatestVersion ? true : false;
                    sshClient.Disconnect();
                }

                if (NewSoftware.Any(b => b == false))
                    LampsToBeUpdated.Add(lampIP);

            }
        }
        catch (Exception e)
        {
            ErrorText.text = e.ToString();
        }
        
        return LampsToBeUpdated;
    }

    public IEnumerator LampsUpdate()
    {
        int lampNumber = 1;
        bool rebootNeeded = false;
        yield return null;
        while (lampsToBeUpdated.Count > 0)
        {
            ProgressBarValue = 0.0f;
            ProgressBar.value = ProgressBarValue;
            UpdateTextValue = string.Format("Updating lamps {0}/{1}, please wait...", lampNumber, UpdateCount);
            UpdateText.text = UpdateTextValue;
            yield return null;

            string UpdateLampIP = lampsToBeUpdated[0];

            bool NumpyNotInstalled = false;
            bool isRev3 = false;
            try
            {
                //Check for latest version of Numpy
                using (SshClient sshClient = new SshClient(UpdateLampIP, LampUsername, LampPassword))
                {
                    sshClient.Connect();

                    //TODO: Check for Rev2 or Rev3
                    var testRevisionCommand = sshClient.RunCommand("uname -r");
                    isRev3 = testRevisionCommand.Result == "4.4.30-pro\n" ? false : true;

                    if (!isRev3)
                    {
                        var TestStatusCommand = sshClient.RunCommand("dpkg -s python3-numpy | grep Status");
                        NumpyNotInstalled = TestStatusCommand.Result == "Status: install ok installed\n" ? false : true;
                    }
                    sshClient.Disconnect();
                }

                //Copy files to device
                using (SftpClient sftpClient = new SftpClient(UpdateLampIP, LampUsername, LampPassword))
                {
                    //Connection
                    sftpClient.Connect();

                    //Numpy
                    if (NumpyNotInstalled)
                    {
                        if (!sftpClient.Exists(NumpyDestinationFolder))
                            sftpClient.CreateDirectory(NumpyDestinationFolder);

                        sftpClient.ChangeDirectory(NumpyDestinationFolder);
                        foreach (var filename in NumpyInstallationFiles)
                        {
                            UploadFileFromResources(filename, sftpClient);
                        }
                    }
                    else
                    {
                        Debug.Log("Numpy already installed.");
                    }

                    //Networking for rev2
                    if (!isRev3)
                    {
                        sftpClient.ChangeDirectory(NetworkFolder);
                        UploadFileFromResources(NetworkFile, sftpClient);
                    }

                    string animDestFolder = isRev3 ? AnimationDestinationFolder3 : AnimationDestinationFolder;

                    //Animation
                    if (!sftpClient.Exists(animDestFolder))
                        sftpClient.CreateDirectory(animDestFolder);

                    sftpClient.ChangeDirectory(animDestFolder);
                    foreach (var filename in AnimationInstallationFiles)
                    {
                        UploadFileFromResources(filename, sftpClient);
                    }

                    //Rev3 update!
                    if (isRev3)
                    {
                        if (!sftpClient.Exists(Rev3InstallationDirectory))
                            sftpClient.CreateDirectory(Rev3InstallationDirectory);

                        sftpClient.ChangeDirectory(Rev3InstallationDirectory);
                        foreach (var filename in BundleInstallationFiles)
                        {
                            UploadFileFromResources(filename, sftpClient);
                        }
                    }
                    else
                    {
                        string udpFile = isRev3 ? UDPfile3 : UDPfile;

                        //ut2
                        sftpClient.ChangeDirectory(AutorunDestinationFolder);
                        UploadFileFromResources(udpFile, sftpClient);
                    }

                    //Autorun
                    string autorunDestFolder = isRev3 ? Rev3InstallationDirectory : AutorunDestinationFolder;
                    string autorunFile = isRev3 ? AutorunSourceFile3 : AutorunSourceFile;

                    sftpClient.ChangeDirectory(autorunDestFolder);
                    UploadFileFromResources(autorunFile, sftpClient, "autorun.sh");
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                ErrorText.text = e.ToString();
                UpdateTextValue = "Update failed!";
                RetryButtonText = "Retry";
                OkCancelButtonText = "Cancel";
                yield break;
            }
            ProgressBarValue = 0.2f;
            ProgressBar.value = ProgressBarValue;
            yield return null;

            try
            {
                using (SshClient sshClient = new SshClient(UpdateLampIP, LampUsername, LampPassword))
                {
                    //Connection
                    sshClient.Connect();

                    //Numpy
                    if (NumpyNotInstalled)
                    {
                        sshClient.RunCommand("mount -o remount,rw /dev/ubi0_0 / -t ubifs");
                        sshClient.RunCommand("dpkg -i /media/numpy_install_temp/*");
                        var TestStatusCommand = sshClient.RunCommand("dpkg -s python3-numpy | grep Status");

                        if (TestStatusCommand.Result == "Status: install ok installed\n")
                        {
                            Debug.Log("Numpy installation: success");
                        }
                        else
                        {
                            Debug.Log("Numpy installation failed!");
                            throw new Exception("Numpy failed!");
                        }
                        sshClient.RunCommand("rm -r /media/numpy_install_temp");
                    }

                    if (isRev3)
                    {
                        Debug.Log("Starting update on lamp.");

                        string dos2unixCommand = "dos2unix " + Rev3InstallationDirectory + "/*.py " + Rev3InstallationDirectory + "/*.sh " + Rev3InstallationDirectory + "/*.chk ";
                        var TransformResult = sshClient.RunCommand(dos2unixCommand);
                        var InstallationCommand = sshClient.CreateCommand("python3 " + Rev3InstallationDirectory + "/update3.py");
                        var InstallationScriptResult = InstallationCommand.BeginExecute();

                        bool installationSuccess = false;
                        using (var reader = new StreamReader(InstallationCommand.OutputStream,Encoding.UTF8,true,1024))
                        {
                            string output = null;
                            while ((output = reader.ReadLine()) != null)
                            {
                                Debug.Log(output);
                                if (output.Contains("UPDATE SUCCESSFUL"))
                                {
                                    installationSuccess = true;
                                }
                                if (output.Contains("REBOOT"))
                                {
                                    rebootNeeded = true;
                                }
                            }
                        }

                        if (!installationSuccess)
                        {
                            Debug.Log("Something went wrong when running installation script on lamp.");
                            throw new Exception("Update for rev3 failed!");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                ErrorText.text = e.ToString();
                UpdateTextValue = "Update failed!";
                RetryButtonText = "Retry";
                OkCancelButtonText = "Cancel";
                yield break;
            }
            ProgressBarValue = 0.9f;
            ProgressBar.value = ProgressBarValue;
            yield return null;

            try
            {
                using (SshClient sshClient = new SshClient(UpdateLampIP, LampUsername, LampPassword))
                {
                    //Connection
                    sshClient.Connect();
                    //Animation
                    //TODO: Run version control command
                    string[] testCommandStrings = new string[] { }; // { "ls -l /media/animation/AnimationPlayer.py | awk '{print $5}'", "ls -l /media/animation/PythonReceiver.py | awk '{print $5}'", "ls -l /media/autorun.sh | awk '{print $5}'", "ls -l /media/ut2.5.py | awk '{print $5}'" };

                    sshClient.RunCommand("dos2unix /mnt/data/animation/*");

                    if (isRev3)
                    {
                        testCommandStrings = new string[] { "ls -l /mnt/data/animation/AnimationPlayer.py | awk '{print $5}'", "ls -l /mnt/data/animation/PythonReceiver.py | awk '{print $5}'", "ls -l /mnt/data/animation/HueCalibration.csv | awk '{print $5}'", "ls -l /mnt/data/animation/IntensityCalibration.csv | awk '{print $5}'", "ls -l /mnt/data/animation/TemperatureCalibration.csv | awk '{print $5}'" };
                    }
                    else
                    {
                        testCommandStrings = new string[] { "ls -l /media/animation/AnimationPlayer.py | awk '{print $5}'", "ls -l /media/animation/PythonReceiver.py | awk '{print $5}'", "ls -l /media/autorun.sh | awk '{print $5}'", "ls -l /media/ut2.5.py | awk '{print $5}'" };
                    }

                    foreach (var testCommand in testCommandStrings)
                    {
                        var TestStatusCommand = sshClient.RunCommand(testCommand);
                        if (Convert.ToInt32(TestStatusCommand.Result) > 0)
                        {
                            Debug.Log("Animation module installation: success");
                        }
                        else
                        {
                            Debug.Log("Animation module and autorun installation failed!");
                            throw new Exception("Animation and autorun failed!");
                        }
                    }
                    Debug.Log("Animation module and autorun installation: success");

                    //if (isRev3)
                    //{
                    //    var UpdateCommand = sshClient.RunCommand("sh /mnt/data/update1.sh");
                    //    if (UpdateCommand.Result != "update successful\n")
                    //    {
                    //        throw new Exception("Update script (sh) failed!");
                    //    }
                    //}


                    Debug.Log("Update has finished, in order for update to take effect, reboot the device!");

                    //Reboot to start autorun
                    //sshClient.RunCommand("reboot");
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                ErrorText.text = e.ToString();
                UpdateTextValue = "Update failed!";
                RetryButtonText = "Retry";
                OkCancelButtonText = "Cancel";
                yield break;
            }
            ProgressBarValue = 1.0f;
            ProgressBar.value = ProgressBarValue;
            yield return null;

            lampNumber++;
            lampsToBeUpdated.Remove(UpdateLampIP);

            if (lampsToBeUpdated.Count == 0)
            {
                UpdateTextValue = "Update completed!";
                if (!isRev3 || rebootNeeded)
                {
                    UpdateTextValue += " Please reboot lamps!";
                }
                OkCancelButtonText = "Ok";
            }
            yield return null;

        }
        yield break;
    }

    private static void UploadFile(string SourceFile1, SftpClient sftpClient)
    {
        using (FileStream fs = new FileStream(SourceFile1, FileMode.Open))
        {
            sftpClient.BufferSize = 4 * 1024;
            sftpClient.UploadFile(fs, Path.GetFileName(SourceFile1));
        }
    }

    private static void UploadFileFromResources(string SourceFile, SftpClient sftpClient, string DestinationFileName = "")
    {
        var resource = Resources.Load(SourceFile) as TextAsset;
        string destFilename = DestinationFileName == "" ? Path.GetFileName(SourceFile) : DestinationFileName;

        if (resource.text.Length == 0)
        {
            //Binary asset
            using (Stream s = new MemoryStream(resource.bytes))
            {
                sftpClient.BufferSize = 4 * 1024;
                sftpClient.UploadFile(s, destFilename);
            }
        }
        else
        {
            //Text asset
            using (Stream s = GenerateStreamFromString(resource.text))
            {
                sftpClient.BufferSize = 4 * 1024;
                sftpClient.UploadFile(s, destFilename);
            }
        }
        
    }

    public static Stream GenerateStreamFromString(string s)
    {
        MemoryStream stream = new MemoryStream();
        StreamWriter writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }
}

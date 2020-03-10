using UnityEditor;
using System;

public class MenuItems
{
    [MenuItem("Tools/Increment Version Up")]
    private static void IncrementVersionUp()
    {
        int androidVersion = PlayerSettings.Android.bundleVersionCode;
        string iosVersion = PlayerSettings.iOS.buildNumber;

        string[] splitVersionString = iosVersion.Split('.');
        int newVersionNumber = Int32.Parse(splitVersionString[2]) + 1;
        string newVersionString = splitVersionString[0] + "." + splitVersionString[1] + "." + newVersionNumber;

        PlayerSettings.iOS.buildNumber = newVersionString;
        PlayerSettings.bundleVersion = newVersionString;
        PlayerSettings.Android.bundleVersionCode = androidVersion + 1;
        PlayerSettings.macOS.buildNumber = (androidVersion + 1).ToString();
    }

    [MenuItem("Tools/Increment Version Down")]
    private static void IncrementVersionDown()
    {
        int androidVersion = PlayerSettings.Android.bundleVersionCode;
        string iosVersion = PlayerSettings.iOS.buildNumber;

        string[] splitVersionString = iosVersion.Split('.');
        int newVersionNumber = Int32.Parse(splitVersionString[2]) - 1;
        string newVersionString = splitVersionString[0] + "." + splitVersionString[1] + "." + newVersionNumber;

        PlayerSettings.iOS.buildNumber = newVersionString;
        PlayerSettings.bundleVersion = newVersionString;
        PlayerSettings.Android.bundleVersionCode = androidVersion - 1;
        PlayerSettings.macOS.buildNumber = (androidVersion - 1).ToString();
    }
}
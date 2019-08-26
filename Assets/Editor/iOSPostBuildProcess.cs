#if UNITY_IOS

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

public class iOSPostBuildProcess
{
    [PostProcessBuild]
    public static void ChangeXcodePlist(BuildTarget target, string path)
    {
        string plistPath = path + "/Info.plist";
        PlistDocument plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(plistPath));

        PlistElementDict rootDict = plist.root;

        //string version = Application.version;
        //int index = version.IndexOf('-');
        //string shortenVersion = version.Substring(0, index + 1);
        //var buildKey = "CFBundleVersion";
        //rootDict.SetString(buildKey, shortenVersion);
            
        rootDict.SetBoolean("ITSAppUsesNonExemptEncryption", false);

        File.WriteAllText(plistPath, plist.WriteToString());
    }
}
 
#endif
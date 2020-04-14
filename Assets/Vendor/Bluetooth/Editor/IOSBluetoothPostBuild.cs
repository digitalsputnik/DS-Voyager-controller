﻿#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using UnityEditor.iOS.Xcode;
#endif

public class IOSBluetoothPostBuild
{
	const string BLUETOOTH_DESCRIPTION = "This application uses bluetooth to find nearby beacons.";

#if UNITY_IOS
#pragma warning disable 0162
    [PostProcessBuild]
	public static void OnPostprocessBuild(BuildTarget _, string buildPath)
	{
        //string pbxProjectPath = PBXProject.GetPBXProjectPath(buildPath);
        //string plistPath = Path.Combine(buildPath, "Info.plist");
        //PBXProject pbxProject = new PBXProject();
        //pbxProject.ReadFromFile(pbxProjectPath);
        //string targetGUID = pbxProject.GetUnityFrameworkTargetGuid();
        //pbxProject.AddBuildProperty(targetGUID, "OTHER_LDFLAGS", "-framework Photos");
        //pbxProject.AddBuildProperty(targetGUID, "OTHER_LDFLAGS", "-framework MobileCoreServices");
        //pbxProject.AddBuildProperty(targetGUID, "OTHER_LDFLAGS", "-framework ImageIO");
        //pbxProject.RemoveFrameworkFromProject(targetGUID, "Photos.framework");
        //File.WriteAllText(pbxProjectPath, pbxProject.WriteToString());
        //PlistDocument plist = new PlistDocument();
        //plist.ReadFromString(File.ReadAllText(plistPath));
        //PlistElementDict rootDict = plist.root;
        //rootDict.SetString("NSPhotoLibraryUsageDescription", PHOTO_LIBRARY_USAGE_DESCRIPTION);
        //rootDict.SetString("NSPhotoLibraryAddUsageDescription", PHOTO_LIBRARY_USAGE_DESCRIPTION);
        //File.WriteAllText(plistPath, plist.WriteToString());

        string plistPath = Path.Combine(buildPath, "Info.plist");
        var propertyList = new PlistDocument();
		propertyList.ReadFromFile(plistPath);
		propertyList.root.SetString("NSBluetoothAlwaysUsageDescription", BLUETOOTH_DESCRIPTION);
		propertyList.root.SetString("NSBluetoothPeripheralUsageDescription", BLUETOOTH_DESCRIPTION);
		propertyList.WriteToFile(plistPath);
	}
#pragma warning restore 0162
#endif
}
#if UNITY_IOS
#pragma warning disable 0162
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using UnityEditor.iOS.Xcode;

public static class PostBuildProcess
{
	private const string BLUETOOTH_DESCRIPTION = "This application uses bluetooth to find nearby lamps.";
	private const string PHOTO_LIBRARY_USAGE_DESCRIPTION = "Save media to Photos";
	private const string NETWORK_PERMISSION_DESCRIPTION = "This application uses network to get your network ssid for lamps.";
	private const string LOCATION_PERMISSION_DESCRIPTION = "This application uses location to get your network ssid for lamps.";
	
	[PostProcessBuild]
	public static void OnPostprocessBuild(BuildTarget _, string buildPath)
	{
        var plistPath = Path.Combine(buildPath, "Info.plist");
        var propertyList = new PlistDocument();
		propertyList.ReadFromFile(plistPath);
		propertyList.root.SetString("NSBluetoothAlwaysUsageDescription", BLUETOOTH_DESCRIPTION);
		propertyList.root.SetString("NSBluetoothPeripheralUsageDescription", BLUETOOTH_DESCRIPTION);
		propertyList.root.SetString("NSPhotoLibraryUsageDescription", PHOTO_LIBRARY_USAGE_DESCRIPTION);
		propertyList.root.SetString("NSPhotoLibraryAddUsageDescription", PHOTO_LIBRARY_USAGE_DESCRIPTION);
		propertyList.root.SetString("NSLocationAlwaysAndWhenInUseUsageDescription", LOCATION_PERMISSION_DESCRIPTION);
		propertyList.root.SetString("NSLocationWhenInUseUsageDescription", LOCATION_PERMISSION_DESCRIPTION);
		propertyList.root.SetString("NSLocationAlwaysUsageDescription", LOCATION_PERMISSION_DESCRIPTION);
		propertyList.root.SetString("CNCopyCurrentNetworkInfo", NETWORK_PERMISSION_DESCRIPTION);
		propertyList.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);
		propertyList.WriteToFile(plistPath);

		var projectPath = PBXProject.GetPBXProjectPath(buildPath);
		var project = new PBXProject();
		project.ReadFromFile(projectPath);
		var targetGuid = project.GetUnityFrameworkTargetGuid();
		project.AddBuildProperty(targetGuid, "OTHER_LDFLAGS", "-framework Photos");
		project.AddBuildProperty(targetGuid, "OTHER_LDFLAGS", "-framework MobileCoreServices");
		project.AddBuildProperty(targetGuid, "OTHER_LDFLAGS", "-framework ImageIO");
		project.RemoveFrameworkFromProject(targetGuid, "Photos.framework");
		project.WriteToFile(projectPath);
	}
}
#pragma warning restore 0162
#endif
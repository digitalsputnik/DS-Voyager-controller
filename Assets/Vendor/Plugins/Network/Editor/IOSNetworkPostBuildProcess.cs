#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using UnityEditor.iOS.Xcode;
#endif

public class IOSNetworkPostBuildProcess
{
	const string NETWORK_PERMISSION = "This application uses network to get your network ssid for lamps.";
	const string LOCATION_PREMISSION = "This application uses location to get your network ssid for lamps.";

#if UNITY_IOS
#pragma warning disable 0162
	[PostProcessBuild]
	public static void OnPostprocessBuild(BuildTarget _, string buildPath)
	{
		string plistPath = Path.Combine(buildPath, "Info.plist");
		var propertyList = new PlistDocument();
		propertyList.ReadFromFile(plistPath);
		propertyList.root.SetString("NSLocationAlwaysAndWhenInUseUsageDescription", LOCATION_PREMISSION);
		propertyList.root.SetString("NSLocationWhenInUseUsageDescription", LOCATION_PREMISSION);
		propertyList.root.SetString("NSLocationAlwaysUsageDescription", LOCATION_PREMISSION);
		propertyList.root.SetString("CNCopyCurrentNetworkInfo", NETWORK_PERMISSION);
		propertyList.WriteToFile(plistPath);
	}
#pragma warning restore 0162
#endif
}
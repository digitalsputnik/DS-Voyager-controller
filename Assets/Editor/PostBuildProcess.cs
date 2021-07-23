#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using UnityEditor.iOS.Xcode;
#endif

public static class PostBuildProcess
{
	private const string BLUETOOTH_DESCRIPTION = "This application uses bluetooth to find nearby lamps.";
	private const string LOCAL_NETWORK_DESCRIPTION = "This application uses local network to find lamps.";
	
#if UNITY_IOS
	[PostProcessBuild]
	public static void OnPostprocessBuild(BuildTarget _, string buildPath)
	{
        var plistPath = Path.Combine(buildPath, "Info.plist");
        var propertyList = new PlistDocument();
		propertyList.ReadFromFile(plistPath);
		propertyList.root.SetString("NSBluetoothAlwaysUsageDescription", BLUETOOTH_DESCRIPTION);
		propertyList.root.SetString("NSBluetoothPeripheralUsageDescription", BLUETOOTH_DESCRIPTION);
		propertyList.root.SetString("NSLocalNetworkUsageDescription", LOCAL_NETWORK_DESCRIPTION);
		propertyList.WriteToFile(plistPath);
	}
#endif
}
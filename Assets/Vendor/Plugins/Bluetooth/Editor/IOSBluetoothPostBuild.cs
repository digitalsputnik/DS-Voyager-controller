#if UNITY_IOS
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
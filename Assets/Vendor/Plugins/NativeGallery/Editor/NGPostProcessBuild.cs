#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using UnityEditor.iOS.Xcode;
#endif

public class NGPostProcessBuild
{
	private const string PHOTO_LIBRARY_USAGE_DESCRIPTION = "Save media to Photos";

#if UNITY_IOS
#pragma warning disable 0162
	[PostProcessBuild]
	public static void OnPostprocessBuild( BuildTarget target, string buildPath )
	{
		if (target != BuildTarget.iOS) return;
		
		var pbxProjectPath = PBXProject.GetPBXProjectPath(buildPath);
		var plistPath = Path.Combine(buildPath, "Info.plist");

		var pbxProject = new PBXProject();
		pbxProject.ReadFromFile(pbxProjectPath);

		var targetGuid = pbxProject.GetUnityFrameworkTargetGuid();

		pbxProject.AddBuildProperty(targetGuid, "OTHER_LDFLAGS", "-weak_framework Photos" );
		pbxProject.AddBuildProperty(targetGuid, "OTHER_LDFLAGS", "-weak_framework PhotosUI" );
		pbxProject.AddBuildProperty(targetGuid, "OTHER_LDFLAGS", "-framework Photos");
		pbxProject.AddBuildProperty(targetGuid, "OTHER_LDFLAGS", "-framework MobileCoreServices");
		pbxProject.AddBuildProperty(targetGuid, "OTHER_LDFLAGS", "-framework ImageIO");
			
		// pbxProject.SetBuildProperty(targetGuid, "ENABLE_BITCODE", "NO");

		pbxProject.RemoveFrameworkFromProject(targetGuid, "Photos.framework");

		File.WriteAllText(pbxProjectPath, pbxProject.WriteToString());

		var plist = new PlistDocument();
		plist.ReadFromString(File.ReadAllText(plistPath));

		var rootDict = plist.root;
		rootDict.SetString("NSPhotoLibraryUsageDescription", PHOTO_LIBRARY_USAGE_DESCRIPTION);
		rootDict.SetString("NSPhotoLibraryAddUsageDescription", PHOTO_LIBRARY_USAGE_DESCRIPTION);
		rootDict.SetBoolean( "PHPhotoLibraryPreventAutomaticLimitedAccessAlert", true );

		File.WriteAllText(plistPath, plist.WriteToString());
	}
#pragma warning restore 0162
#endif
}
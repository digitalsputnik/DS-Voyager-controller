/* 
*   NatCam
*   Copyright (c) 2018 Yusuf Olokoba
*/

namespace NatCamU.Extensions {

	using UnityEditor;
	using System;

	#if UNITY_IOS
	using UnityEditor.Callbacks;
	using UnityEditor.iOS.Xcode;
	using System.IO;
	#endif

	public static class NatCamEditor {

		private const string
		CameraUsageKey = @"NSCameraUsageDescription",
		CameraUsageDescription = @"Used for Voyager lamp detection", // Change this as necessary
		GalleryUsageKey = @"NSPhotoLibraryUsageDescription",
		GalleryUsageDescription = @"Used for adding background images to lamps";

		#if UNITY_IOS

		[PostProcessBuild]
		static void LinkFrameworks (BuildTarget buildTarget, string path) {
			if (buildTarget != BuildTarget.iOS) return;
			string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
			PBXProject proj = new PBXProject();
			proj.ReadFromString(File.ReadAllText(projPath));
			string target = proj.TargetGuidByName("Unity-iPhone");
			foreach (var framework in new [] { "Accelerate.framework", "CoreImage.framework" })
				proj.AddFrameworkToProject(target, framework, true);
			File.WriteAllText(projPath, proj.WriteToString());
		}

		[PostProcessBuild]
		static void SetPermissions (BuildTarget buildTarget, string path) {
			if (buildTarget != BuildTarget.iOS) return;
			string plistPath = path + "/Info.plist";
			PlistDocument plist = new PlistDocument();
			plist.ReadFromString(File.ReadAllText(plistPath));
			PlistElementDict rootDictionary = plist.root;
			rootDictionary.SetString(CameraUsageKey, CameraUsageDescription);
			rootDictionary.SetString(GalleryUsageKey, GalleryUsageDescription);
			File.WriteAllText(plistPath, plist.WriteToString());
		}
		#endif
	}
}
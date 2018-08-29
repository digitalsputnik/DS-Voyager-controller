using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PhotoCamera))]
public class PhotoCameraEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		PhotoCamera script = (PhotoCamera)target;
		if (GUILayout.Button("Screenshot"))
			script.WorkspacePhoto();
	}
}

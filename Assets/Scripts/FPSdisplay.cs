using UnityEngine;
using System.Collections;
 
public class FPSdisplay : MonoBehaviour
{
	float deltaTime = 0.0f;
	 
	void Update()
	{
		deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
	}
	 
	void OnGUI()
	{
		int w = Screen.width, h = Screen.height;
		 
		GUIStyle style = new GUIStyle();
		 
		Rect rect = new Rect(5, 5, w, h * 2 / 100);
		style.alignment = TextAnchor.UpperLeft;
		style.fontSize = h * 2 / 30;
		style.normal.textColor = Color.white;
		float msec = deltaTime * 1000.0f;
		float fps = 1.0f / deltaTime;
		//string text = string.Format("{0:0.0} ms ({1:0.0} fps)", msec, fps);
		GUI.Label(rect, ((int)fps).ToString()+" fps", style);
	}
}
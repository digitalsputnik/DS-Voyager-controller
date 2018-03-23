using System.Collections;
using UnityEngine;
using UnityEditor;

public class MyTools
{
	//39 & 83
	[MenuItem("MyTools/CreateVoyager 2ft")]
	static void call2ft() {
		Create (39);
	}

	[MenuItem("MyTools/CreateVoyager 4ft")]
	static void call4ft() {
		Create (83);
	}

	static void Create(int count)
	{

		//create null for vayger
		Object vPrefab = AssetDatabase.LoadAssetAtPath("Assets/Valgusti/Voyager.prefab",  typeof(GameObject));
		GameObject Voyager = GameObject.Instantiate (vPrefab, Vector3.zero, Quaternion.identity) as GameObject;

		//create pixels

		Object prefab = AssetDatabase.LoadAssetAtPath("Assets/Valgusti/LEDmodule.prefab", typeof(GameObject));

		for (int x=0; x!=count; x++)
		{
			GameObject light = GameObject.Instantiate (prefab, Vector3.zero, Quaternion.identity) as GameObject;
			//set properties
			light.name = "pixel" + x;
			light.transform.position = new Vector3(x * 1.45f, 0, 0);
			//light.transform.eulerAngles = new Vector3(0, 0, 90);
			light.transform.parent = Voyager.transform;
		}
	}
}

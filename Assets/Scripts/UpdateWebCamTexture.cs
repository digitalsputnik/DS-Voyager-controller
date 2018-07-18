using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateWebCamTexture : MonoBehaviour {

	[SerializeField] DrawScripts drawScripts;
   
	void LateUpdate () {

		drawScripts.UpdateLampVideoStream();
	}
}

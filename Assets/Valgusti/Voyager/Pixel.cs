using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pixel : MonoBehaviour {
	Ribbon myContoller;
	Material geomMaterial;
	Material glowMaterial;
	GameObject selectionPixel;
	public Vector4 ITSH;
	public int ID;
    Color VisibleEmissionColor = new Color(1.0f, 0.56f, 0.1f);
    Color InvisibleEmissionColor = new Color(0.25f, 0.14f, 0.02f);

    // Use this for initialization
    void Awake () {
		myContoller = this.transform.parent.GetComponent<Ribbon> ();
		geomMaterial = this.transform.Find ("LEDmodule").GetComponent<Renderer> ().material;
		glowMaterial = this.transform.Find ("glow").GetComponent<Renderer> ().material;
		selectionPixel = this.transform.Find ("SelectionPixel").gameObject;
		//get ID from assigned name TODO hardcode?
		ID = int.Parse (this.transform.name.Substring (5));
	}
	
	public void updatePixel(Vector4 ITSHin, Color uiColorIn) {
		myContoller.setPixel (ID, ITSHin);
		ITSH = ITSHin;
        //set color of the UI Pixel
        geomMaterial.color = uiColorIn;
        glowMaterial.SetColor("_Color", new Color(uiColorIn.r, uiColorIn.g, uiColorIn.b, 0.3f));
    }

	public void updateSelectionPixel(int val) {
		//Debug.Log ("Changing material color...");
		if (val == 0) {
			selectionPixel.SetActive (false);
		} else if (val == 1) {
            selectionPixel.GetComponent<Renderer>().material.SetColor("_EmissionColor", VisibleEmissionColor);
            selectionPixel.SetActive (true);
		} else if (val == 2) {
			selectionPixel.GetComponent<Renderer> ().material.SetColor ("_EmissionColor", InvisibleEmissionColor);
			selectionPixel.SetActive (true);
		}

	}
}

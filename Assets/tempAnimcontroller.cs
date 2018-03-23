using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tempAnimcontroller : MonoBehaviour {

	public bool animTag;
	public AnimationLayer layer;
	public AnimationCurve ICurve;
	public AnimationCurve TCurve;
	public AnimationCurve SCurve;
	public AnimationCurve HCurve;

	public int oldI;
	public int oldT;
	public int oldS;
	public int oldH;


	public int secI;
	public int secT;
	public int secS;
	public int secH;

	public DrawMode main;

    public string StrokeID;

    public void GenerateNewStrokeID()
    {
        StrokeID = Guid.NewGuid().ToString();
    }

    void Awake()
    {
        GenerateNewStrokeID();
    }

    // Use this for initialization
    void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

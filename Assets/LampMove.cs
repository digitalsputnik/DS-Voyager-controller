using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LampMove : MonoBehaviour {

	public DragHandle handle1, handle2;
	public Transform lampGraphics;
	public PanZoom cameraZoom;

	float lofh; // Lamp offset from handle ( There's a little space between )
	float lz; // Lamp Z position;
	float scaleMul; // Start distance between handles

	int handleCount;

	void Start()
	{
		InitializeEvents();

		cameraZoom = Camera.main.GetComponent<PanZoom>();

		lofh = Vector3.Distance(lampGraphics.position, handle1.transform.position);
		lz = lampGraphics.position.z;
		scaleMul = (Vector3.Distance(handle1.transform.position, handle2.transform.position) - 2 * lofh) / lampGraphics.localScale.x;
	}

	void OnDragStarted(object sender, System.EventArgs e)
	{
		handleCount++;

		cameraZoom.enabled = false;
	}

	void OnDragging(object sender, System.EventArgs e)
	{
		CalculateGraphicsPositionAndRotation();
	}

	void OnDragEnded(object sender, System.EventArgs e)
	{
		handleCount--;

		if (handleCount == 0)
			cameraZoom.enabled = true;
	}

    void InitializeEvents()
	{
		handle1.OnDragStarted += OnDragStarted;
        handle2.OnDragStarted += OnDragStarted;

        handle1.OnDragging += OnDragging;
        handle2.OnDragging += OnDragging;

        handle1.OnDragEnded += OnDragEnded;
        handle2.OnDragEnded += OnDragEnded;
	}

    void CalculateGraphicsPositionAndRotation()
	{
		//Simplified variables
		Vector3 p1 = handle1.transform.position;
        Vector3 p2 = handle2.transform.position;

        // Rotation
        float angle = Mathf.Atan2(p2.y - p1.y, p2.x - p1.x) * 180 / Mathf.PI;
        lampGraphics.eulerAngles = new Vector3(0.0f, 0.0f, angle);

        // Position
        // np as new position
        float npx = p1.x + lofh * Mathf.Cos(angle * Mathf.Deg2Rad);
        float npy = p1.y + lofh * Mathf.Sin(angle * Mathf.Deg2Rad);
		lampGraphics.position = new Vector3(npx, npy, lz);

		// Scale
		float scale = (Vector3.Distance(p1, p2) - 2 * lofh) / scaleMul;
		lampGraphics.localScale = new Vector3(scale, scale, scale);
	}
}
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LampMove : MonoBehaviour {

	public DragHandle moveHandle;
	public DragHandle sizeHandle1, sizeHandle2;
	public Transform lampGraphics;
	public PanZoom cameraZoom;

	float lampOffsetFromHandle;
	float lampZPos;
	float scaleMultiplier;

	int sizeTouchCount;

	Transform sizeHandle1T, sizeHandle2T;
	Vector3 sizeHandle1Offset, sizeHandle2Offset;

	void Start()
	{
		InitializeEvents();
        
		sizeHandle1T = sizeHandle1.transform;
		sizeHandle2T = sizeHandle2.transform;

		cameraZoom = Camera.main.GetComponent<PanZoom>();

		lampOffsetFromHandle = Vector3.Distance(lampGraphics.position, sizeHandle1T.position);
		lampZPos = lampGraphics.position.z;
		scaleMultiplier = (Vector3.Distance(sizeHandle1T.position, sizeHandle2T.position) - 2 * lampOffsetFromHandle) / lampGraphics.localScale.x;
	}

	void MoveOnDragStarted()
	{
		sizeHandle1Offset = lampGraphics.position - sizeHandle1T.position;
		sizeHandle2Offset = lampGraphics.position - sizeHandle2T.position;

		sizeHandle1.enabled = false;
		sizeHandle2.enabled = false;
	}

	void MoveOnDragging()
	{
		sizeHandle1T.position = lampGraphics.position - sizeHandle1Offset;
		sizeHandle2T.position = lampGraphics.position - sizeHandle2Offset;
	}

	void MoveOnDragEnded()
	{
		sizeHandle1.enabled = true;
        sizeHandle2.enabled = true;
	}

	void SizeOnDragStarted()
	{
		sizeTouchCount++;

		cameraZoom.enabled = false;
	}

	void SizeOnDragging()
	{
		CalculateGraphicsPositionAndRotation();
	}

	void SizeOnDragEnded()
	{
		sizeTouchCount--;

		if (sizeTouchCount == 0)
			cameraZoom.enabled = true;
	}

    void InitializeEvents()
	{
		moveHandle.OnDragStarted += MoveOnDragStarted;
		moveHandle.OnDragging += MoveOnDragging;
		moveHandle.OnDragEnded += MoveOnDragEnded;

		sizeHandle1.OnDragStarted += SizeOnDragStarted;
		sizeHandle1.OnDragging += SizeOnDragging;
        sizeHandle1.OnDragEnded += SizeOnDragEnded;

        sizeHandle2.OnDragStarted += SizeOnDragStarted;
        sizeHandle2.OnDragging += SizeOnDragging;
		sizeHandle2.OnDragEnded += SizeOnDragEnded;

	}

    void CalculateGraphicsPositionAndRotation()
	{
		//Simplified variables
		Vector3 p1 = sizeHandle1T.position;
		Vector3 p2 = sizeHandle2T.position;

        // Rotation
        float angle = Mathf.Atan2(p2.y - p1.y, p2.x - p1.x) * 180 / Mathf.PI;
        lampGraphics.eulerAngles = new Vector3(0.0f, 0.0f, angle);

        // Position
        // np as new position
        float npx = p1.x + lampOffsetFromHandle * Mathf.Cos(angle * Mathf.Deg2Rad);
        float npy = p1.y + lampOffsetFromHandle * Mathf.Sin(angle * Mathf.Deg2Rad);
		lampGraphics.position = new Vector3(npx, npy, lampZPos);

		// Scale
		float scale = (Vector3.Distance(p1, p2) - 2 * lampOffsetFromHandle) / scaleMultiplier;
		lampGraphics.localScale = new Vector3(scale, scale, scale);
	}
}
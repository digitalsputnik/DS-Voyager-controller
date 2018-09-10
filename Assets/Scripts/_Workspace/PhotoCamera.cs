using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Voyager.Workspace;

public class PhotoCamera : MonoBehaviour {

	public List<Transform> targets;
	public Vector3 offset;
	public float minZoom, maxZoom;
	RenderTexture texture;

	void Start()
	{
		texture = new RenderTexture(600, 400, 16, RenderTextureFormat.ARGB32);
		texture.Create();
		GetComponent<Camera>().targetTexture = texture;
		InvokeRepeating("UpdateTargets", 0.0f, 2.0f);
	}

	public Texture2D WorkspacePhoto()
	{                     
		Texture2D texture2d = new Texture2D(texture.width, texture.height);
		OpenCVForUnity.Utils.textureToTexture2D(texture, texture2d); 
		return texture2d;
	}

    void UpdateTargets()
	{
		List<WorkspaceItem> items = Workspace.GetItemsInWorkspace();
        targets.Clear();
        for (int i = 0; i < items.Count; i++)
        {
			if (items[i].Type == WorkspaceItem.WorkspaceItemType.Lamp)
				targets.Add(items[i].transform.GetChild(0).Find("Center"));
			else
				targets.Add(items[i].transform.GetChild(0).GetChild(0));
        }

		Move();
        Zoom();
	}

	void Move()
	{
        Vector3 position = GetCenterPoint();
        transform.position = position + offset;
	}

    void Zoom()
	{
		if (targets.Count == 0)
            return;

		Bounds bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
            bounds.Encapsulate(targets[i].position);

		float zoom = (bounds.size.x > bounds.size.y) ? bounds.size.x : bounds.size.y;
		zoom /= 50.0f;
		GetComponent<Camera>().fieldOfView = Mathf.Clamp(zoom, minZoom, maxZoom);
	}

	Vector3 GetCenterPoint()
	{
		if (targets.Count == 0)
			return Vector3.zero;

		if(targets.Count == 1)
			return targets[0].position + new Vector3(0.0f, 0.0f, -5.0f);

		Bounds bounds = new Bounds(targets[0].position, Vector3.zero);
		for (int i = 0; i < targets.Count; i++)
			bounds.Encapsulate(targets[i].position);
        
		Vector3 returnValue = bounds.center;
		returnValue.z = -((bounds.size.y + bounds.size.x) + 5.0f);

		return returnValue;
	}
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkspaceItem : MonoBehaviour {

	[Space(5)]
	[SerializeField] GameObject[] hideableItems;
	[SerializeField] MonoBehaviour[] hideableComponents;
	[SerializeField] Collider[] hideableColliders;
	[Space(3)]
	public WorkspaceItemType Type;

	public WorkspaceItem parent;
	public List<WorkspaceItem> children = new List<WorkspaceItem>();

    public void ShowGraphics()
	{
		foreach (GameObject item in hideableItems)
			item.SetActive(true);

		foreach (MonoBehaviour item in hideableComponents)
			item.enabled = true;

		foreach (Collider coll in hideableColliders)
			coll.enabled = true;
	}
       
    public void HideGraphics()
	{
		foreach (GameObject item in hideableItems)
			item.SetActive(false);

		foreach (MonoBehaviour item in hideableComponents)
			item.enabled = false;

		foreach (Collider coll in hideableColliders)
            coll.enabled = false;
	}

	public void SetParent(WorkspaceItem parent)
	{
		this.parent = parent;
		if (!this.parent.children.Contains(this))
			this.parent.children.Add(this);
	}

	public WorkspaceItem GetChild(int index)
	{
		if (index < children.Count) return children[index];

		return null;
	}

	public enum WorkspaceItemType
	{
		Lamp,
        Image,
        Video
	}
}
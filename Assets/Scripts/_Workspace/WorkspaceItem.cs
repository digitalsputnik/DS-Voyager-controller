using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkspaceItem : MonoBehaviour {

	public MonoBehaviour[] hideableItems;
	public WorkspaceItemType Type;

	public WorkspaceItem parent;
	public List<WorkspaceItem> children = new List<WorkspaceItem>();

    public void Show()
	{
		foreach (MonoBehaviour item in hideableItems)
            item.enabled = true;
	}

    public void Hide()
	{
		foreach (MonoBehaviour item in hideableItems)
			item.enabled = true;
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
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Voyager.Workspace;

public class WorkspaceItem : MonoBehaviour {

	[Space(5)]
	[SerializeField] GameObject[] hideableItems;
	[SerializeField] MonoBehaviour[] hideableComponents;
	[SerializeField] Collider[] hideableColliders;
	[Space(3)]
	public WorkspaceItemType Type;
       
	public WorkspaceItem parent;
	public List<WorkspaceItem> children = new List<WorkspaceItem>();
    
	Vector3 lastMove;
    Vector3 lastSize;

	LampMove move;

	void Start()
	{
		move = GetComponent<LampMove>();
		lastMove = move.lampGraphics.position;
		lastSize = move.lampGraphics.localScale;
	}

	void Update()
	{
		if (move.scaling)
		{
			foreach(WorkspaceItem wi in children)
			{
				LampMove childMove = wi.GetComponent<LampMove>();
				childMove.SetupOffsets();
			}
		}
	}

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
		if(parent == null && this.parent != null)
		{
			if (this.parent.children.Contains(this))
                this.parent.children.Remove(this);
		}

		if (this.parent != null)
		{
			if (this.parent.children.Contains(this))
				this.parent.children.Remove(this);
        }
            
        this.parent = parent;
		if (this.parent == null)
		{
			transform.SetParent(Workspace.GetWorkspaceTransform());
			return; 
		}

		if (!this.parent.children.Contains(this))
			this.parent.children.Add(this);

		transform.SetParent(this.parent.GetComponent<LampMove>().lampGraphics);
		this.parent.GetComponent<LampMove>().SetupOffsets();
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
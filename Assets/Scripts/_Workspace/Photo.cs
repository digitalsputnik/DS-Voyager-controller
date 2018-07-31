using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Voyager.Workspace
{
	public class Photo : MonoBehaviour
    {
		WorkspaceItem item;
		LampMove movingLamp;
		[SerializeField] Collider col;
		public string photoName;
		public Texture2D texture;

		void Start()
		{
			item = GetComponent<WorkspaceItem>();
		}

		void Update()
		{
			CheckForMovingLamps();
		}

		void CheckForMovingLamps()
		{
			foreach (WorkspaceItem wi in Workspace.GetItemsInWorkspace())
			{
				if(wi.Type == WorkspaceItem.WorkspaceItemType.Lamp)
				{
					LampMove move = wi.GetComponent<LampMove>();
                    if (move.moving)
                    {
                        movingLamp = move;
                        return;
                    }
				}
			}

			if(movingLamp != null)
			{
				MovingEnded();
				movingLamp = null;
            }
		}

		void MovingEnded()
		{
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			Physics.Raycast(ray, out hit, 1000);
			col.enabled = true;
			WorkspaceItem workspaceItem = movingLamp.GetComponent<WorkspaceItem>();
			if (col.bounds.Contains(hit.point)) workspaceItem.SetParent(item);
			else
			{
				if (workspaceItem.parent == item)
					workspaceItem.SetParent(null);
			}
			col.enabled = false;
		}
	}
}
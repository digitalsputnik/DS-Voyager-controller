using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Voyager.Workspace
{
	public class Photo : MonoBehaviour
    {
		[SerializeField] Collider col;
		public string photoName;
		public Texture2D texture;
		public Color outlineColor;

        WorkspaceItem item;
        LampMove movingLamp;
		GameObject outline;

		void Start()
		{
			item = GetComponent<WorkspaceItem>();         
		}

		public void Setup(Texture2D texture, string photoName)
        {
			this.texture = texture;
			this.photoName = photoName;
            LampMove move = GetComponent<LampMove>();
			              
			int width = texture.width;
			int height = texture.height;
			float aspect = height / (float)width;
			Transform image = move.lampGraphics.GetChild(0);
			image.localScale = new Vector3(image.localScale.x, 30 * aspect, 1.0f);

			BoxCollider[] colliders = move.lampGraphics.GetComponents<BoxCollider>();
			foreach(BoxCollider coll in colliders)
				coll.size = new Vector3(coll.size.x, 30 * aspect, coll.size.z);

			outline = move.lampGraphics.GetChild(0).GetChild(0).gameObject;
			Vector3 outlineScale = Vector3.one;
			outlineScale.x += 0.02f;
			outlineScale.y += 0.02f / aspect;
			outline.transform.localScale = outlineScale;
			outline.GetComponent<MeshRenderer>().material.color = outlineColor;
            outline.SetActive(false);
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
						DrawOutline();
                        return;
                    }
				}
			}

			if(movingLamp != null)
			{
				MovingEnded();
				outline.SetActive(false);
				movingLamp = null;
            }
		}

        void DrawOutline()
		{
			RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out hit, 1000);
            col.enabled = true;
            WorkspaceItem workspaceItem = movingLamp.GetComponent<WorkspaceItem>();
			outline.SetActive(col.bounds.Contains(hit.point));
            col.enabled = false;
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
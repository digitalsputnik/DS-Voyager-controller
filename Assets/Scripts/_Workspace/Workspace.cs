using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Voyager.Lamps;

namespace Voyager.Workspace
{
	public class Workspace : MonoBehaviour
    {
		static Workspace instance;
        [SerializeField] GameObject[] SpawnableItems;
		[SerializeField] List<WorkspaceItem> ItemsInWorkspace = new List<WorkspaceItem>();
        
		void Start()
		{
			instance = this;
		}

		public static void ShowGraphics()
        {
            foreach (WorkspaceItem item in instance.ItemsInWorkspace)
                item.ShowGraphics();
        }

		public static void HideGraphics()
		{
			foreach (WorkspaceItem item in instance.ItemsInWorkspace)
				item.HideGraphics();
		}
              
		public static PhysicalLamp InstantiateLamp(Lamp lamp)
        {
            return InstantiateLamp(lamp, Vector3.zero);
        }

        public static PhysicalLamp InstantiateLamp(Lamp lamp, Vector3 position)
        {
            if (lamp.physicalLamp != null)
            {
                Debug.LogError("Lamp allready in a scene.");
                return null;
            }

            GameObject lampPrefab = GetLampPrefab(lamp.Type);
            if (lampPrefab == null)
            {
                Debug.LogError("This type of lamp has no prefab in \"SpawnableItems\" list.");
                return null;
            }

			GameObject physicalObject = InstantiateItem(lampPrefab, position);
            PhysicalLamp physicalLamp = physicalObject.GetComponent<PhysicalLamp>();
            physicalLamp.Setup(lamp);
            lamp.physicalLamp = physicalLamp;

            return physicalLamp;
        }

        public static PhysicalLamp InstantiateLamp(Lamp lamp, Vector3 handle1, Vector3 handle2)
        {
            PhysicalLamp physical = InstantiateLamp(lamp);
            physical.GetComponent<LampMove>().SetPosition(handle1, handle2);
            return physical;
        }

		public static void DestroyItem(WorkspaceItem item)
		{
			if (item.Type == WorkspaceItem.WorkspaceItemType.Lamp)
				item.GetComponent<PhysicalLamp>().Owner.physicalLamp = null;

			instance.ItemsInWorkspace.Remove(item);
            Destroy(item.gameObject);
        }

		internal static void DestroyLamp(PhysicalLamp physicalLamp)
		{
			physicalLamp.Owner.physicalLamp = null;
			instance.ItemsInWorkspace.Remove(physicalLamp.GetComponent<WorkspaceItem>());
			Destroy(physicalLamp.gameObject);
		}

        static GameObject InstantiateItem(GameObject prefab, Vector3 position, WorkspaceItem parent = null)
        {
            GameObject item = Instantiate(prefab, position, Quaternion.identity, instance.transform);
            instance.ItemsInWorkspace.Add(item.GetComponent<WorkspaceItem>());
            return item;
        }

        static GameObject GetLampPrefab(LampType type)
        {
            PhysicalLamp physicalLamp;
            foreach (GameObject go in instance.SpawnableItems)
            {
                WorkspaceItem item = go.GetComponent<WorkspaceItem>();
                if(item.Type == WorkspaceItem.WorkspaceItemType.Lamp)
                {
                    physicalLamp = go.GetComponent<PhysicalLamp>();
                    if (physicalLamp.Type == type)
                        return go;
                }
            }
            return null;
        }

	}
}
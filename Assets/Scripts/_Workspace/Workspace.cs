using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using Voyager.Lamps;

namespace Voyager.Workspace
{
	public class Workspace : MonoBehaviour
    {
		static Workspace instance;
        [SerializeField] GameObject[] SpawnableItems;
		[SerializeField] List<WorkspaceItem> ItemsInWorkspace = new List<WorkspaceItem>();
        
		void Awake()
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

		public static Transform InstantiateVideoStream()
		{
			return InstantiateItem(GetVideoStreamPrefab(), Vector3.forward * 0.7f).transform;
		}

		static GameObject InstantiateItem(GameObject prefab, Vector3 position, WorkspaceItem parent = null)
        {
            GameObject item = Instantiate(prefab, position, Quaternion.identity, instance.transform);
            instance.ItemsInWorkspace.Add(item.GetComponent<WorkspaceItem>());
            return item;
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

        public static bool ContainsVideoStream()
		{
			foreach (WorkspaceItem item in instance.ItemsInWorkspace)
				if (item.Type == WorkspaceItem.WorkspaceItemType.Video) return true;

			return false;
		}

		public static Transform GetVideoSteam()
		{
			foreach (WorkspaceItem item in instance.ItemsInWorkspace)
				if (item.Type == WorkspaceItem.WorkspaceItemType.Video) return item.transform;

            return null;
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

		static GameObject GetVideoStreamPrefab()
		{
			foreach (GameObject go in instance.SpawnableItems)
            {
                WorkspaceItem item = go.GetComponent<WorkspaceItem>();
				if (item.Type == WorkspaceItem.WorkspaceItemType.Video)
					return go;
            }
            return null;
		}

		public static void SaveWorkplace()
        {
            string filename = "/workplace_" + SceneManager.GetActiveScene().name + ".dat";
            SaveWorkplace(filename);
        }

		public static void SaveWorkplace(string filename)
        {
            BinaryFormatter bf = new BinaryFormatter();

			List<Lamp> lampsInWorkplace = new List<Lamp>();
			foreach (WorkspaceItem item in instance.ItemsInWorkspace) 
			{
				if (item.Type == WorkspaceItem.WorkspaceItemType.Lamp)
					lampsInWorkplace.Add(item.GetComponent<PhysicalLamp>().Owner);
			}

            LampSaveData[] lampSaveData = new LampSaveData[lampsInWorkplace.Count];

            for (int i = 0; i < lampsInWorkplace.Count; i++)
            {
                Lamp lamp = lampsInWorkplace[i];
                LampMove lampMove = lamp.physicalLamp.GetComponent<LampMove>();
                Vector3 handle1 = lampMove.sizeHandle1.transform.position;
                Vector3 handle2 = lampMove.sizeHandle2.transform.position;

                LampSaveData lampData = new LampSaveData
                {
                    serial = lamp.Serial,
                    ip = lamp.IP.ToString(),
                    type = (int)lamp.Type,
                    lenght = lamp.Lenght,
                    colordata = lamp.pixelColorData,
                    handle1 = new SerVector3(handle1),
                    handle2 = new SerVector3(handle2)
                };

                lampSaveData[i] = lampData;
            }

            WorkplaceData data = new WorkplaceData() { lamps = lampSaveData };

            FileStream file = File.Create(Application.persistentDataPath + filename);
            bf.Serialize(file, data);
            file.Close();
            file.Dispose();
        }

		public static void LoadWorkplace()
        {
            string filename = "/workplace_" + SceneManager.GetActiveScene().name + ".dat";
            LoadWorkplace(filename);
        }

		public static void LoadWorkplace(string filename)
        {
            if (File.Exists(Application.persistentDataPath + filename))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + filename, FileMode.Open);
                WorkplaceData data = (WorkplaceData)bf.Deserialize(file);
                file.Close();
                file.Dispose();

                LampSaveData[] lampDataArray = data.lamps;
                List<Lamp> returnLamps = new List<Lamp>();

				LampManager lampManager = GameObject.FindWithTag("LampManager").GetComponent<LampManager>();

                foreach (LampSaveData lampData in lampDataArray)
                {
                    Lamp lamp = null;
					if (lampManager.LampExists(lampData.serial))
                    {
						lamp = lampManager.GetLamp(lampData.serial);

						if (lampManager.LampExistsInWorkplace(lamp.Serial))
                            DestroyLamp(lamp.physicalLamp);
                    }
                    else
                    {
                        lamp = new Lamp();
                        lamp.Setup(lampData.serial, lampData.ip, (LampType)lampData.type, lampData.lenght, lampData.colordata);
						lampManager.GetLamps().Add(lamp);
                    }
                    InstantiateLamp(lamp, lampData.handle1.ToVector3(), lampData.handle2.ToVector3());
                }
            }
        }

		[Serializable]
        public struct WorkplaceData
        {
            public LampSaveData[] lamps;
        }
	}
}
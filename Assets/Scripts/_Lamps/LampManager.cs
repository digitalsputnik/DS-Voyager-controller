using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Voyager.Networking;
using Newtonsoft.Json;
using UnityEngine;
using System.Net;
using System.IO;
using System;

namespace Voyager.Lamps
{
	public class LampManager : MonoBehaviour
	{
		[SerializeField] List<Lamp> Lamps = new List<Lamp>();
		[Header("Settings")]
		[SerializeField] float LampsLookInterval = 0.2f;
		[SerializeField] float RegisterDeviceInterval = 0.1f;
		[SerializeField] float LampTimeoutTime = 10.0f;
		[SerializeField] Transform workSpace;
        [Space(3)]
		[SerializeField] GameObject[] PhysicalLamps;

		void Start()
		{
			NetworkManager.OnAvailableLampsResponse += NetworkManager_OnAvailableLampsResponse;
			NetworkManager.OnLampColorResponse += NetworkManager_OnLampColorResponse;

			InvokeRepeating("LookForAvailableLamps", 0.0f, LampsLookInterval);
			InvokeRepeating("CheckLampsTimeout", 0.0f, RegisterDeviceInterval);
			InvokeRepeating("RegisterDevices", 0.0f, RegisterDeviceInterval);

			if (workSpace == null) workSpace = transform;
		}      
        
		void NetworkManager_OnAvailableLampsResponse(string response, IPAddress ip)
        {
			ReplyUdpResponse lampData = JsonConvert.DeserializeObject<ReplyUdpResponse>(response);
			Debug.Log("Lamp data received from " + ip);
			UpdateOrAddLamp(lampData);
        }

		void NetworkManager_OnLampColorResponse(byte[] data, IPAddress ip)
        {
			Debug.Log("Lamp colors received from " + ip);
            UpdateLampColors(data, ip);
        }

		void LookForAvailableLamps()
		{
			NetworkManager.AskAvailableLamps();
		}

        void RegisterDevices()
		{
			foreach(Lamp lamp in GetLampsInWorkplace())
				NetworkManager.RegisterDevice(lamp.IP);
		}

        void CheckLampsTimeout()
		{
			foreach (Lamp lamp in Lamps) lamp.CheckTimeout(LampTimeoutTime);
		}

		void UpdateLampColors(byte[] data, IPAddress ip)
		{
			Lamp lamp = GetLamp(ip);
			if (lamp != null && data.Length > 0) lamp.Update(data);
		}

		void UpdateOrAddLamp(ReplyUdpResponse response)
		{
			if (SerialExistsInLamps(response.serial_name))
			{
				Lamp lamp = GetLamp(response.serial_name);
                lamp.Update(response);
			}
			else
			{
                Lamp lamp = new Lamp();
                lamp.Setup(response);
                Lamps.Add(lamp);
				NewLampAdded(lamp);
			}         
	    }

        void NewLampAdded(Lamp lamp)
		{
			NetworkManager.RegisterDevice(lamp.IP);
		}

		public Lamp GetLamp(string serial)
		{
			foreach (var lamp in Lamps)
            {
				if (lamp.Serial == serial)
					return lamp;
            }

			return null;
		}

        public Lamp GetLamp(int index)
		{
			if(index < Lamps.Count)
			    return Lamps[index];

			return null;
		}

		public Lamp GetLamp(IPAddress ip)
		{
			foreach (var lamp in Lamps)
				if (lamp.IP.ToString() == ip.ToString())
                    return lamp;
            return null;
		}

        public List<Lamp> GetConnectedLamps()
		{
			List<Lamp> lamps = new List<Lamp>();
			foreach (Lamp lamp in Lamps)
				if (!lamp.ConnectionLost)
					lamps.Add(lamp);
			return lamps;
		}
        
		public List<Lamp> GetLampsInWorkplace()
        {
            List<Lamp> lamps = new List<Lamp>();
            foreach (Lamp lamp in Lamps)
				if (lamp.physicalLamp != null)
                    lamps.Add(lamp);
            return lamps;
        }

        public List<Lamp> GetAddableLamps()
		{
			List<Lamp> lamps = new List<Lamp>();
            foreach (Lamp lamp in Lamps)
				if (lamp.physicalLamp == null && !lamp.ConnectionLost && lamp.updateChecked)
                    lamps.Add(lamp);
            return lamps;
		}

		public List<Lamp> GetUncheckedLamps()
        {
            List<Lamp> lamps = new List<Lamp>();
            foreach (Lamp lamp in Lamps)
				if (!lamp.updateChecked && !lamp.updatingFirmware)
                    lamps.Add(lamp);
            return lamps;
        }

		public List<Lamp> GetLamps()
		{
			return Lamps;
		}

        public bool LampExistsInWorkplace(string serial)
        {
            List<Lamp> lamps = GetLampsInWorkplace();
            foreach (Lamp lamp in lamps)
                if (lamp.Serial == serial)
                       return true;
            return false;
        }

        public bool LampExists(string serial)
		{
			return (GetLamp(serial) != null);
		}

		public PhysicalLamp InstantiateLamp(Lamp lamp)
		{
			return InstantiateLamp(lamp, Vector3.zero);
		}

		public PhysicalLamp InstantiateLamp(Lamp lamp, Vector3 position)
		{
			if(lamp.physicalLamp != null)
			{
				Debug.LogError("Lamp allready in a scene.");
				return null;
			}
			
			GameObject lampPrefab = GetLampPrefab(lamp.Type);
			if(lampPrefab == null)
			{
				Debug.LogError("This type of lamp has no prefab in PlysicalLamps list.");
				return null;
			}

			GameObject physicalObject = Instantiate(lampPrefab, position, Quaternion.identity, workSpace);
			PhysicalLamp physicalLamp = physicalObject.GetComponent<PhysicalLamp>();
            physicalLamp.Setup(lamp);
			lamp.physicalLamp = physicalLamp;

			return physicalLamp;
		}

		public PhysicalLamp InstantiateLamp(Lamp lamp, Vector3 handle1, Vector3 handle2)
		{
			PhysicalLamp physical = InstantiateLamp(lamp);
			physical.GetComponent<LampMove>().SetPosition(handle1, handle2);
			return physical;
		}

		public void DestroyLamp(PhysicalLamp lamp)
		{
			lamp.Owner.physicalLamp = null;
			Destroy(lamp.gameObject);
		}

		public GameObject GetLampPrefab(LampType type)
		{
			PhysicalLamp physicalLamp;
			foreach(GameObject go in PhysicalLamps)
			{
				physicalLamp = go.GetComponent<PhysicalLamp>();
				if (physicalLamp.Type == type)
					return go;
			}
			return null;
		}
        
		bool SerialExistsInLamps(string serial)
		{
			foreach (var lamp in Lamps)
			{
				if (lamp.Serial == serial)
					return true;
			}

			return false;
		}
	
		public void SaveWorkplace()
		{
			string filename = "/workplace_" + SceneManager.GetActiveScene().name + ".dat";
			SaveWorkplace(filename);
		}

		public void SaveWorkplace(string filename)
		{
			BinaryFormatter bf = new BinaryFormatter();

			List<Lamp> lampsInWorkplace = GetLampsInWorkplace();
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

		public void LoadWorkplace()
		{
			string filename = "/workplace_" + SceneManager.GetActiveScene().name + ".dat";
			LoadWorkplace(filename);
		}

        public void LoadWorkplace(string filename)
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

				foreach(LampSaveData lampData in lampDataArray)
				{
					Lamp lamp = null;
					if (LampExists(lampData.serial))
					{
						lamp = GetLamp(lampData.serial);

						if (LampExistsInWorkplace(lamp.Serial))
							DestroyLamp(lamp.physicalLamp);             
					}
					else
					{
						lamp = new Lamp();
						lamp.Setup(lampData.serial, lampData.ip, (LampType)lampData.type, lampData.lenght, lampData.colordata);
						Lamps.Add(lamp);
					}
                                   
					InstantiateLamp(lamp, lampData.handle1.ToVector3(), lampData.handle2.ToVector3());
				}
            }
		}

        public void SetGraphicsCollision(bool value)
		{
			foreach(Lamp lamp in GetLampsInWorkplace())
				lamp.physicalLamp.transform.Find("Graphics").GetComponent<BoxCollider>().enabled = value;
		}

        public void HideGraphics(bool value)
		{
			foreach(Lamp lamp in GetLampsInWorkplace())
			{
				PhysicalLamp physicalLamp = lamp.physicalLamp;
				physicalLamp.Text.gameObject.SetActive(!value);
				physicalLamp.move.sizeHandle1.gameObject.SetActive(!value);
				physicalLamp.move.sizeHandle2.gameObject.SetActive(!value);
			}
		}

		[Serializable]
		public struct WorkplaceData
		{
			public LampSaveData[] lamps;
		}
	}
}
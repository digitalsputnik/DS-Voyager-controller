using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Voyager.Networking;
using Voyager.Workspace;
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
		[Space(3)]
		[SerializeField] bool Debugging;

		void Start()
		{
			NetworkManager.OnAvailableLampsResponse += NetworkManager_OnAvailableLampsResponse;
			NetworkManager.OnLampColorResponse += NetworkManager_OnLampColorResponse;

			InvokeRepeating("LookForAvailableLamps", 0.0f, LampsLookInterval);
			InvokeRepeating("CheckLampsTimeout", 0.0f, RegisterDeviceInterval);
			InvokeRepeating("RegisterDevices", 0.0f, RegisterDeviceInterval);
		}      
        
		void NetworkManager_OnAvailableLampsResponse(string response, IPAddress ip)
        {
			ReplyUdpResponse lampData = JsonConvert.DeserializeObject<ReplyUdpResponse>(response);
			if(Debugging) Debug.Log("Lamp data received from " + ip + " - " + response);
			UpdateOrAddLamp(lampData);
        }

		void NetworkManager_OnLampColorResponse(byte[] data, IPAddress ip)
        {
			if(Debugging) Debug.Log("Lamp colors received from " + ip);
            UpdateLampColors(data, ip);
        }

		void LookForAvailableLamps()
		{
			NetworkManager.AskAvailableLamps();
		}

        void RegisterDevices()
		{
			foreach(Lamp lamp in GetConnectedLamps())
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
				if (lamp.physicalLamp == null && !lamp.ConnectionLost && lamp.upToDate)
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
        
		bool SerialExistsInLamps(string serial)
		{
			foreach (var lamp in Lamps)
			{
				if (lamp.Serial == serial)
					return true;
			}

			return false;
		}
	}
}
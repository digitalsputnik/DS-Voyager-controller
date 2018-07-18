using Voyager.Networking;
using UnityEngine;
using System.Net;
using System;

namespace Voyager.Lamps
{
	[Serializable]
	public class Lamp
    {
		public LampType Type;
		public string Name;
		public string Serial;
		public IPAddress IP;
		public int Lenght;
		public int BatteryLevel;
		public bool ConnectionLost;

        public string Mode;
        public string activePattern;
        public string activePatternPassword;
        public string activeSSID;
        public string activePassword;
        public int activeChannel;

        public int[] animationVersion;
        public int hardwareVersion;
        public int[] lpcVersion;
        public int[] chipVersion;

		public PhysicalLamp physicalLamp;
		public DateTime lastUpdate;
		public byte[] pixelColorData;

		public bool updateChecked;
        public bool updatingFirmware;

		public void Setup(ReplyUdpResponse response)
		{
			LampType lampType = PickLampType(response.length);
            string lampName = PickLampName(response.length);

			Type = lampType;
			Name = lampName;
			Serial = response.serial_name;
			IP = new IPAddress(response.IP);
			Lenght = response.length;
			BatteryLevel = response.battery_level;
			Mode = response.active_mode;

			activePattern = response.active_pattern;
			activePatternPassword = response.active_pattern_ps;

			activeSSID = response.active_ssid;
			activePassword = response.active_password;
			activeChannel = response.active_channel;

			animationVersion = response.animation_version;
			hardwareVersion = response.hardware_version;
			lpcVersion = response.LPC_version;
			chipVersion = response.CHIP_version;
        }

		public void Setup(string serial, string ip, LampType type, int lenght, byte[] colordata)
		{
			Serial = serial;
			Lenght = lenght;
			IP = IPAddress.Parse(ip);
			Type = type;
			pixelColorData = colordata;
		}

		public void Update(ReplyUdpResponse response)
        {
            IP = new IPAddress(response.IP);
            BatteryLevel = response.battery_level;
			Mode = response.active_mode;

			activePattern = response.active_pattern;
            activePatternPassword = response.active_pattern_ps;

            activeSSID = response.active_ssid;
            activePassword = response.active_password;
            activeChannel = response.active_channel;

            animationVersion = response.animation_version;
            hardwareVersion = response.hardware_version;
            lpcVersion = response.LPC_version;
            chipVersion = response.CHIP_version;

            lastUpdate = DateTime.Now;
        }

        public void Update(byte[] colorData)
		{
			pixelColorData = colorData;
			lastUpdate = DateTime.Now;
		}

		public void CheckTimeout(float timeoutTime)
		{
			DateTime now = DateTime.Now;
			ConnectionLost = (now.TimeOfDay.TotalSeconds - lastUpdate.TimeOfDay.TotalSeconds >= timeoutTime);
		}

		LampType PickLampType(int length)
        {
            switch (length)
            {
                case 39:
                    return LampType.Voyager_2ft;
                case 42:
                    return LampType.Voyager3_2ft;
                case 82:
                    return LampType.Voyager_4ft;
                case 83:
                    return LampType.Voyager3_4ft;
                case 3:
					return LampType.DS3_x3;
                case 18:
                    return LampType.DSBeam;
            }

			return LampType.None;
        }

        string PickLampName(int length)
        {
            switch (length)
            {
                case 39:
                    return "Short Voyager";
                case 42:
                    return "Short Voyager";
                case 82:
                    return "Long Voyager";
                case 83:
                    return "Long Voyager";
                case 3:
                    return "DS3 x 3";
                case 18:
                    return "DSBeam";
            }

            return "";
        }
    }

	[Serializable]
	public enum LampType
	{
		Voyager_2ft,
		Voyager_4ft,
		Voyager3_2ft,
        Voyager3_4ft,
        DS3_x3,
        DSBeam,
        None
	}

	[Serializable]
    public struct LampSaveData
	{
		public string serial { get; set; }
		public int lenght { get; set; }
		public string ip { get; set; }
		public int type { get; set; }
		public byte[] colordata { get; set; }
		public SerVector3 handle1 { get; set; }
		public SerVector3 handle2 { get; set; }
	}
    
	[Serializable]
	public struct SerVector3
	{
		public float x { get; set; }
		public float y { get; set; }
		public float z { get; set; }

		public SerVector3(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public SerVector3(Vector3 vector)
		{
			x = vector.x;
			y = vector.y;
			z = vector.z;
		}

		public Vector3 ToVector3()
		{
			return new Vector3(x, y, z);
		}
	}
}
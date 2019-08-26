using System.Net;
using System.Text;
using UnityEngine;
using VoyagerApp.Dmx;
using VoyagerApp.Networking;

namespace VoyagerApp.Lamps.Voyager
{
    internal class VoyagerDataProcessor : LampDataProcessor
    {
        VoyagerUpdateUtility updateUtility;

        internal VoyagerDataProcessor(LampManager manager) : base(manager)
        {
            updateUtility = new VoyagerUpdateUtility();

            var voyagerClient = new VoyagerClient(NetworkManager.instance);
            voyagerClient.onReceived += VoyagerDataReceived;
            NetworkManager.instance.AddClient(voyagerClient);
        }

        bool IsLampInfoResponse(byte[] data) => DataContains(data, "serial_name");
        bool IsDmxSettingsResponse(byte[] data) => DataContains(data, "DMXmode");

        void VoyagerDataReceived(object sender, byte[] data)
        {
            if (IsLampInfoResponse(data))
                HandleResponseData(data);
            else if (IsDmxSettingsResponse(data))
                HandleDmxResponseData(data, sender);
        }

        void HandleResponseData(byte[] data)
        {
            var packed = VoyagerLampInfoResponse.FromData(data);
            Lamp lamp = manager.GetLampWithSerial(packed.serial);

            if (lamp == null)
                CreateLamp(packed);
            else
                lamp.Update(packed);
        }

        void HandleDmxResponseData(byte[] data, object sender)
        {
            var settings = DmxSettings.FromData(data);
            var address = ((IPEndPoint)sender).Address;
            Lamp lamp = manager.GetLampWithAddress(address);
            lamp.UpdateDmxSettings(settings);
        }

        bool DataContains(byte[] data, string str)
        {
            string json = Encoding.UTF8.GetString(data);
            return json.Contains(str);
        }

        void CreateLamp(VoyagerLampInfoResponse packed)
        {
            VoyagerLamp lamp = new VoyagerLamp();
            lamp.Update(packed);
            manager.AddLamp(lamp);
        }
    }
}
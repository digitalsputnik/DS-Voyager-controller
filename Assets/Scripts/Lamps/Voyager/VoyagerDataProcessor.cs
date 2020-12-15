using System.Net;
using System.Text;
using VoyagerApp.Networking;
using VoyagerApp.Networking.Voyager;

namespace VoyagerApp.Lamps.Voyager
{
    internal class VoyagerDataProcessor : LampDataProcessor
    {
        internal VoyagerDataProcessor(LampManager manager) : base(manager)
        {
            var voyagerClient = new VoyagerClient(NetworkManager.instance);
            voyagerClient.onReceived += VoyagerDataReceived;
            NetworkManager.instance.AddClient(voyagerClient);
        }

        bool IsLampInfoResponse(byte[] data) => DataContains(data, "serial_name");
        bool IsLampBroadcast(byte[] data) => DataContains(data, "side_button_click");
        bool IsDmxSettingsResponse(byte[] data) => DataContains(data, "dmx_mode_response");

        void VoyagerDataReceived(object sender, byte[] data)
        {
            if (IsLampBroadcast(data))
                HandleBroadcast(data);
            else if (IsLampInfoResponse(data))
                HandleResponseData(data);
            else if (IsDmxSettingsResponse(data))
                HandleDmxResponseData(data, sender);
        }

        void HandleResponseData(byte[]data)
        {
            var packed = VoyagerLampInfoResponse.FromData(data);
            Lamp lamp = manager.GetLampWithSerial(packed.serial);

            if (lamp == null)
                CreateLamp(packed);
            else
                lamp.Update(packed);
        }

        void HandleBroadcast(byte[] data)
        {
            var packed = ActivateVideoTrigger.FromData(data);
            var lamp = manager.GetLampWithSerial(packed.serial);

            if (lamp != null)
                MainThread.Dispach(() => BroadcastLamp(lamp));
        }

        void HandleDmxResponseData(byte[] data, object sender)
        {
            var packet = Packet.Deserialize<DmxModeResponse>(data);
            if (packet != null)
            {
                var address = ((IPEndPoint)sender).Address;
                Lamp lamp = null;
                if (string.IsNullOrEmpty(packet.serial))
                    lamp = manager.GetLampWithAddress(address);
                else
                    lamp = manager.GetLampWithSerial(packet.serial);
                lamp?.Update(packet);
            }
        }

        bool DataContains(byte[]data, string str)
        {
            var json = Encoding.UTF8.GetString(data);
            return json.Contains(str);
        }

        void CreateLamp(VoyagerLampInfoResponse packed)
        {
            VoyagerLamp lamp = new VoyagerLamp();
            lamp.Update(packed);
            manager.AddLamp(lamp);
        }

        void BroadcastLamp(Lamp lamp) => manager.LampBroadcasted(lamp);
    }
}
using DigitalSputnik;

namespace VoyagerController.Bluetooth
{
    public class BluetoothEndPoint : LampEndPoint
    {
        public string Id { get; set; }

        public BluetoothEndPoint(string id) => Id = id;
    }
}
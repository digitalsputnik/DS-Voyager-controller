using DigitalSputnik;

namespace VoyagerController
{
    public class BluetoothEndpoint : LampEndPoint
    {
        public string Id { get; set; }

        public BluetoothEndpoint(string id) => Id = id;
    }
}
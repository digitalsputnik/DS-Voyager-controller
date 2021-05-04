using System.IO.Ports;
using DigitalSputnik;

namespace VoyagerController.Serial
{
    public class SerialEndPoint : LampEndPoint
    {
        public SerialPort Stream { get; set; }
    }
}
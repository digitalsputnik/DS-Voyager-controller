using System.Text;
using DigitalSputnik;
using DigitalSputnik.Colors;
using DigitalSputnik.Voyager;
using DigitalSputnik.Voyager.Communication;
using DigitalSputnik.Voyager.Json;
using Newtonsoft.Json;

namespace VoyagerController.Serial
{
    public class VoyagerSerialClient : VoyagerClient
    {
        public override double TimeOffset => 0.0f;

        public VoyagerSerialClient()
        {
            
        }
        
        private static byte[] ObjectToBytes(object obj)
        {
            var json = JsonConvert.SerializeObject(obj, new ItsheConverter());
            return Encoding.UTF8.GetBytes(json);
        }
        
        #region Implementation
        public override void SetItshe(VoyagerLamp voyager, Itshe itshe)
        {
            var packet = new SetItshePacket(itshe);
            SendSettingsPacket(voyager, packet, TimeUtils.Epoch);
        }

        public override double StartStream(VoyagerLamp voyager)
        {
            return 0.0f;
        }

        public override void SendStreamFrame(VoyagerLamp voyager, double time, double index, Rgb[] frame)
        {
            
        }

        public override double StartVideo(VoyagerLamp voyager, long frameCount, double startTime = -1)
        {
            return 0.0f;
        }

        public override void SendVideoFrame(VoyagerLamp voyager, long index, double time, Rgb[] frame)
        {
            
        }

        public override void OverridePixels(VoyagerLamp voyager, Itshe itshe, double duration)
        {
            
        }

        public override void SetFps(VoyagerLamp voyager, double fps)
        {
            
        }

        public override void SetNetworkMode(VoyagerLamp voyager, NetworkMode mode, string ssid = "", string password = "")
        {
            
        }

        public override void SetGlobalIntensity(VoyagerLamp voyager, double value)
        {
            
        }

        public override void SendSettingsPacket(VoyagerLamp voyager, Packet packet, double time)
        {
            
        }

        public override void PollAvailableSsidList(VoyagerLamp voyager, SsidListHandler callback)
        {
            
        }
        #endregion
    }
}
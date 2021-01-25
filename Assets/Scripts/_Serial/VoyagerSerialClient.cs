using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DigitalSputnik;
using DigitalSputnik.Colors;
using DigitalSputnik.Dmx;
using DigitalSputnik.Voyager;
using DigitalSputnik.Voyager.Communication;
using DigitalSputnik.Voyager.Json;
using Newtonsoft.Json;
using UnityEngine;

namespace VoyagerController.Serial
{
    public class VoyagerSerialClient : VoyagerClient
    {
        private static readonly byte[] CRC8_TABLE = {
            0x00, 0x5e, 0xbc, 0xe2, 0x61, 0x3f, 0xdd, 0x83, 0xc2, 0x9c, 0x7e, 0x20, 0xa3, 0xfd, 0x1f, 0x41, 0x9d, 0xc3,
            0x21, 0x7f, 0xfc, 0xa2, 0x40, 0x1e, 0x5f, 0x01, 0xe3, 0xbd, 0x3e, 0x60, 0x82, 0xdc, 0x23, 0x7d, 0x9f, 0xc1,
            0x42, 0x1c, 0xfe, 0xa0, 0xe1, 0xbf, 0x5d, 0x03, 0x80, 0xde, 0x3c, 0x62, 0xbe, 0xe0, 0x02, 0x5c, 0xdf, 0x81,
            0x63, 0x3d, 0x7c, 0x22, 0xc0, 0x9e, 0x1d, 0x43, 0xa1, 0xff, 0x46, 0x18, 0xfa, 0xa4, 0x27, 0x79, 0x9b, 0xc5,
            0x84, 0xda, 0x38, 0x66, 0xe5, 0xbb, 0x59, 0x07, 0xdb, 0x85, 0x67, 0x39, 0xba, 0xe4, 0x06, 0x58, 0x19, 0x47,
            0xa5, 0xfb, 0x78, 0x26, 0xc4, 0x9a, 0x65, 0x3b, 0xd9, 0x87, 0x04, 0x5a, 0xb8, 0xe6, 0xa7, 0xf9, 0x1b, 0x45,
            0xc6, 0x98, 0x7a, 0x24, 0xf8, 0xa6, 0x44, 0x1a, 0x99, 0xc7, 0x25, 0x7b, 0x3a, 0x64, 0x86, 0xd8, 0x5b, 0x05,
            0xe7, 0xb9, 0x8c, 0xd2, 0x30, 0x6e, 0xed, 0xb3, 0x51, 0x0f, 0x4e, 0x10, 0xf2, 0xac, 0x2f, 0x71, 0x93, 0xcd,
            0x11, 0x4f, 0xad, 0xf3, 0x70, 0x2e, 0xcc, 0x92, 0xd3, 0x8d, 0x6f, 0x31, 0xb2, 0xec, 0x0e, 0x50, 0xaf, 0xf1,
            0x13, 0x4d, 0xce, 0x90, 0x72, 0x2c, 0x6d, 0x33, 0xd1, 0x8f, 0x0c, 0x52, 0xb0, 0xee, 0x32, 0x6c, 0x8e, 0xd0,
            0x53, 0x0d, 0xef, 0xb1, 0xf0, 0xae, 0x4c, 0x12, 0x91, 0xcf, 0x2d, 0x73, 0xca, 0x94, 0x76, 0x28, 0xab, 0xf5,
            0x17, 0x49, 0x08, 0x56, 0xb4, 0xea, 0x69, 0x37, 0xd5, 0x8b, 0x57, 0x09, 0xeb, 0xb5, 0x36, 0x68, 0x8a, 0xd4,
            0x95, 0xcb, 0x29, 0x77, 0xf4, 0xaa, 0x48, 0x16, 0xe9, 0xb7, 0x55, 0x0b, 0x88, 0xd6, 0x34, 0x6a, 0x2b, 0x75,
            0x97, 0xc9, 0x4a, 0x14, 0xf6, 0xa8, 0x74, 0x2a, 0xc8, 0x96, 0x15, 0x4b, 0xa9, 0xf7, 0xb6, 0xe8, 0x0a, 0x54,
            0xd7, 0x89, 0x6b, 0x35
        };

        public override double TimeOffset => 0.0f;
        
        public override Type EndpointType => typeof(SerialEndPoint);

        public VoyagerSerialClient()
        {
            Task.Delay(1000).ContinueWith(t => Scan());
        }

        private void Scan()
        {
            foreach (var portName in AvailableComPorts())
            {
                var serialPort = new SerialPort(portName, 750000, Parity.None, 8, StopBits.One)
                {
                    Handshake = Handshake.None,
                    RtsEnable = false,
                    ReadTimeout = 1,
                    WriteBufferSize = 2048
                };
                
                //serialPort.Open();

                try 
                {
                    if (!serialPort.IsOpen)
                        serialPort.Open();
                }
                catch(Exception ex)
                {
                    Debug.Log(ex);
                }

                var lamp = new VoyagerLamp
                {
                    Endpoint = new SerialEndPoint { Stream = serialPort },
                    Serial = "DS" + portName,
                    PixelCount = 42,
                    Connected = true
                };

                AddLampToManager(lamp);
            }
        }

        private static void SendMessage(VoyagerLamp voyager, string message)
        {
            if (voyager.Endpoint is SerialEndPoint endpoint)
            {
                if (endpoint.Stream.IsOpen)
                {
                    var packet = AssembleVoyagerPacket(message);
                    endpoint.Stream.Write(packet, 0, packet.Length);
                    Debug.Log($"[{packet.Length}] " + Encoding.UTF8.GetString(packet));
                }
            }
        }
        
        private static byte[] AssembleVoyagerPacket(string text)
        {
            //header and footer
            var header = new byte[] { 0xAA, 0x03 };
            var footer = new byte[] { 0xEF, 0xFE };

            //Packet assembly (header + packet + footer + CRC8)
            var packetContent = Encoding.UTF8.GetBytes(text);
            var initialPacket = new byte[header.Length + packetContent.Length + footer.Length];
            var finalPacket = new byte[initialPacket.Length + 1]; //added crc8

            Buffer.BlockCopy(header, 0, initialPacket, 0, header.Length);
            Buffer.BlockCopy(packetContent, 0, initialPacket, header.Length, packetContent.Length);
            Buffer.BlockCopy(footer, 0, initialPacket, header.Length + packetContent.Length, footer.Length);

            //Calculating and adding CRC8
            var crc8 = ComputeCRC8Checksum(initialPacket);
            Buffer.BlockCopy(initialPacket, 0, finalPacket, 0, initialPacket.Length);
            finalPacket[finalPacket.Length - 1] = crc8;

            return finalPacket;
        }

        private static byte ComputeCRC8Checksum(byte[] bytes)
        {
            byte crc = 0xFF; //Seed value
            if (bytes != null && bytes.Length > 0)
            {
                var length = bytes.Length & 0xff;
                for (var i = 0; i < length; i++)
                    crc ^= CRC8_TABLE[crc ^ bytes[i]];
            }

            return 0x0;
            // return crc;
        }
        
        #region Implementation
        public override void SetItshe(VoyagerLamp voyager, Itshe itshe)
        {
            var msgBuilder = new StringBuilder();

            msgBuilder.Append("{\"itshe\":{\"e\":");
            msgBuilder.Append(itshe.E.ToString(CultureInfo.InvariantCulture));
            msgBuilder.Append(",\"h\":");
            msgBuilder.Append(itshe.H.ToString(CultureInfo.InvariantCulture));
            msgBuilder.Append(",\"i\":");
            msgBuilder.Append(itshe.I.ToString(CultureInfo.InvariantCulture));
            msgBuilder.Append(",\"s\":");
            msgBuilder.Append(itshe.S.ToString(CultureInfo.InvariantCulture));
            msgBuilder.Append(",\"t\":");
            msgBuilder.Append(itshe.T.ToString(CultureInfo.InvariantCulture));
            msgBuilder.Append("},\"op_code\":\"set_itshe\",\"timestamp\":");
            msgBuilder.Append(TimeUtils.Epoch.ToString(CultureInfo.InvariantCulture));
            msgBuilder.Append("}");

            var json = msgBuilder.ToString().Replace(' ', '\0');

            var packet = new SetItshePacket(itshe);
            
            SendSettingsPacket(voyager, packet, TimeUtils.Epoch);
            
            // SendMessage(voyager, json);
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
            packet.Timestamp = time;
            var json = JsonConvert.SerializeObject(packet, new ItsheConverter()).Replace(' ', '\0');
            SendMessage(voyager, json);
        }

        public override void SendDiscoveryPacket(VoyagerLamp voyager, Packet packet, double time)
        {
            throw new NotImplementedException();
        }

        public override void PollAvailableSsidList(VoyagerLamp voyager, SsidListHandler callback)
        {
            
        }
        
        public override void SetDmxMode(VoyagerLamp voyager, DmxSettings settings)
        {
            
        }

        public override void Dispose()
        {
            foreach (var lamp in LampManager.Instance.Lamps)
            {
                if (lamp.Endpoint is SerialEndPoint serial)
                    serial.Stream.Close();
            }
        }

        #endregion
        
        #region Utilities

        private static string[] AvailableComPorts()
        {
            if (Application.platform == RuntimePlatform.WindowsEditor ||
                Application.platform == RuntimePlatform.WindowsPlayer)
            {
                return SerialPort.GetPortNames();
            }

            
            if (Application.platform == RuntimePlatform.OSXEditor || 
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                var ports = new List<string>();
                var p = (int)Environment.OSVersion.Platform;
                if (p != 4 && p != 128 && p != 6) return ports.ToArray();
                var ttys = System.IO.Directory.GetFiles ("/dev/", "tty.*");
                ports.AddRange(ttys.Where(dev => dev.StartsWith("/dev/tty.")));
                return ports.ToArray();
            }

            return new string[0];
        }
        
        #endregion
    }
}
﻿#if UNITY_IOS
using System;
using UnityEngine;

namespace DigitalSputnik.Bluetooth
{
    public class IOSBluetoothListener : MonoBehaviour
    {
        internal event IosPeripheralScanHandler OnPeripheralScanned;
        internal event IosPeripheralIdHandler OnConnectingStarted;
        internal event IosPeripheralIdHandler OnPeripheralNotFound;
        internal event IosPeripheralIdHandler OnConnectingSuccessful;
        internal event IosPeripheralIdErrorHandler OnConnectingFailed;
        internal event IosPeripheralIdErrorHandler OnDisconnect;
        internal event IosServicesHandler OnServices;
        internal event IosCharacteristicsHandler OnCharacteristics;
        internal event IosCharacteristicUpdateHandler OnCharacteristicUpdate;

        #region Callbacks From iOS
        public void PeripheralScanned(string raw)
        {
            var fields = raw.Split('|');
            var name = fields[1] == "(null)" ? "Unknown" : fields[1];
            OnPeripheralScanned?.Invoke(fields[0], name, int.Parse(fields[2]));
        }

        public void ConnectingStarted(string raw)
        {
            OnConnectingStarted?.Invoke(raw);
        }

        public void PeripheralNotFound(string raw)
        {
            OnPeripheralNotFound?.Invoke(raw);
        }

        public void ConnectionSuccessful(string raw)
        {
            OnConnectingSuccessful?.Invoke(raw);
        }

        public void ConnectionFailed(string raw)
        {
            string[] fields = raw.Split('|');
            string error = fields[1] == "(null)" ? "" : fields[1];
            OnConnectingFailed?.Invoke(fields[0], error);
        }

        public void Disconnect(string raw)
        {
            string[] fields = raw.Split('|');
            string error = fields[1] == "(null)" ? "" : fields[1];
            OnDisconnect?.Invoke(fields[0], error);
        }

        public void GetServices(string raw)
        {
            string[] fields = raw.Split('|');
            string[] services = fields[1].Split('#');
            string error = fields[2] == "(null)" ? "" : fields[2];
            OnServices?.Invoke(fields[0], services, error);
        }

        public void GetCharacteristics(string raw)
        {
            string[] fields = raw.Split('|');
            string[] characteristics = fields[2].Split('#');
            string error = fields[3] == "(null)" ? "" : fields[3];
            OnCharacteristics?.Invoke(fields[0], fields[1], characteristics, error);
        }

        public void UpdateCharacteristic(string raw)
        {
            string[] fields = raw.Split('|');
            string id = fields[0];
            string service = fields[1];
            string characteristic = fields[2];
            string error = fields[3] == "(null)" ? "" : fields[3];


            if (string.IsNullOrEmpty(error))
            {
                var stringData = "";
                for (var i = 4; i < fields.Length; i++)
                    stringData += fields[i];
                var data = Convert.FromBase64String(stringData);
                OnCharacteristicUpdate?.Invoke(id, service, characteristic, error, data);
            }
            else
            {
                OnCharacteristicUpdate?.Invoke(id, service, characteristic, error, null);
            }
        }
        #endregion
    }

    internal delegate void IosPeripheralScanHandler(string peripheral, string name, int rssi);
    internal delegate void IosPeripheralIdHandler(string peripheral);
    internal delegate void IosPeripheralIdErrorHandler(string peripheral, string error);
    internal delegate void IosServicesHandler(string peripheral, string[] services, string error);
    internal delegate void IosCharacteristicsHandler(string peripheral, string service, string[] characteristics, string error);
    internal delegate void IosCharacteristicUpdateHandler(string peripheral, string service, string characteristic, string error, byte[] data);
}
#endif
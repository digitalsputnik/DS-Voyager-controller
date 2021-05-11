#if UNITY_IOS
namespace DigitalSputnik.Ble
{
    internal class IosConnectionSession
    {
        public readonly InternalPeripheralConnectHandler OnConnected;
        public readonly InternalPeripheralConnectFailHandler OnConnectFailed;
        public readonly InternalPeripheralDisconnectHandler OnDisconnect;        
        public InternalServicesHandler OnServices;
        public InternalCharacteristicHandler OnCharacteristics;
        public InternalCharacteristicUpdateHandler OnCharacteristicUpdate;

        public bool Connected { get; set; }
        public bool Connecting { get; set; }

        public IosConnectionSession(InternalPeripheralConnectHandler onConnected, InternalPeripheralConnectFailHandler onConnectFailed, InternalPeripheralDisconnectHandler onDisconnect)
        {
            OnConnected = onConnected;
            OnConnectFailed = onConnectFailed;
            OnDisconnect = onDisconnect;
            Connecting = true;
        }
    }
}
#endif
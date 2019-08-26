using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VoyagerApp.Networking
{
    public class NetworkManager : MonoBehaviour
    {
        #region Singleton
        public static NetworkManager instance;
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this);
            }
            else
                Destroy(this);
        }
        #endregion

        List<LampClient> lampClients = new List<LampClient>();

        void Update() => lampClients.ForEach(_ => _.Receive());

        public T GetLampClient<T>() where T : LampClient
        {
            return (T)lampClients.First(_ => _ is T);
        }

        public void AddClient(LampClient client)
        {
            lampClients.Add(client);
        }
    }

    public abstract class LampClient
    {
        public event ByteHandler onReceived;

        public abstract void Send(byte[] data, object info);
        public abstract void Receive();

        protected void InvokeReceived(object sender, byte[] data)
        {
            onReceived?.Invoke(sender, data);
        }
    }

    public delegate void ByteHandler(object sender, byte[] data);
}
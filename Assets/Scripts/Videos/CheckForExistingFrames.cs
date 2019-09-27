using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using VoyagerApp.Lamps;
using VoyagerApp.Networking.Packages;
using VoyagerApp.Networking.Packages.Voyager;
using VoyagerApp.Utilities;

namespace VoyagerApp.UI
{
    public class CheckForExistingFrames : MonoBehaviour
    {
        [SerializeField] float riskFactor = 0.05f;
        [SerializeField] float requestTime = 1.0f;

        Dictionary<Lamp, bool> lampToRendered = new Dictionary<Lamp, bool>();
        public bool allRendered => lampToRendered.All(p => p.Value == true);

        public void Start()
        {
            InvokeRepeating("CheckForFrames", 0.5f, requestTime);
        }

        public void Clear()
        {
            foreach (var lamp in lampToRendered.Keys)
                lampToRendered[lamp] = false;
        }

        void CheckForFrames()
        {
            foreach (var lamp in WorkspaceUtils.Lamps)
                StartCoroutine(IEnumCheckForLampFrames(lamp));
        }

        IEnumerator IEnumCheckForLampFrames(Lamp lamp)
        {
            if (lamp.video == null)
            {
                lampToRendered[lamp] = true;
            }
            else
            {
                var client = NetUtils.VoyagerClient;
                NetUtils.VoyagerClient.onReceived += OnReceived;

                long[] missing = new long[0];
                var endPacket = new MissingFramesRequestPacket();
                client.SendPacket(lamp, endPacket);

                float starttime = Time.time;
                bool responseReceived = false;

                yield return new WaitUntil(() =>
                {
                    float passed = Time.time - starttime;
                    bool timeout = passed > requestTime;
                    bool over = timeout || responseReceived;
                    if (over) NetUtils.VoyagerClient.onReceived -= OnReceived;
                    return over;
                });

                void OnReceived(object sender, byte[] data)
                {
                    try
                    {
                        IPEndPoint endpoint = (IPEndPoint)sender;
                        if (endpoint.Address.ToString() == lamp.address.ToString())
                        {
                            var packet = Packet.Deserialize<MissingFramesResponsePacket>(data);
                            missing = packet.indices;
                            responseReceived = true;
                        }
                    }
                    catch (System.Exception) { }
                }

                lampToRendered[lamp] = missing.Length < lamp.buffer.frames * riskFactor;
            }
        }
    }
}
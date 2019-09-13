using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoyagerApp.Lamps;
using VoyagerApp.Networking.Packages;
using VoyagerApp.Networking.Packages.Voyager;
using VoyagerApp.UI.Overlays;
using VoyagerApp.Utilities;

namespace VoyagerApp.Projects
{
    public class ProjectBufferSender
    {
        int startCount;
        int finished;
        Queue<Lamp> lamps;
        LoadingBarProcess loadingProcess;
        MonoBehaviour behaviour;

        public ProjectBufferSender(Lamp[] lamps, MonoBehaviour behaviour)
        {
            startCount = lamps.Length;
            this.lamps = new Queue<Lamp>(lamps);
            this.behaviour = behaviour;
        }

        public void StartSending()
        {
            loadingProcess = LoadingBar.CreateLoadProcess(
                "Sending video buffers to lamps");
            StartSendingToNextLamp();
        }

        void StartSendingToNextLamp()
        {
            if (lamps.Count > 0)
            {
                Lamp lamp = lamps.Dequeue();
                if (lamp.connected)
                    SendBufferToLamp(lamp);
                else
                    SendingToLampFinished();
            }
        }

        void SendBufferToLamp(Lamp lamp)
        {
            List<long> indices = new List<long>();
            for (long i = 0; i < lamp.buffer.frames; i++)
            {
                if (lamp.buffer.FrameExists(i))
                    indices.Add(i);
            }

            var frames = new Dictionary<long, byte[]>();
            foreach (var i in indices)
                frames.Add(i, lamp.buffer.GetFrame(i));

            lamp.SetVideo(lamp.video);
            behaviour.StartCoroutine(SendFramesToLamp(lamp, frames));
        }

        IEnumerator SendFramesToLamp(Lamp lamp, Dictionary<long, byte[]> frames)
        {
            var client = NetUtils.VoyagerClient;
            var indices = frames.Keys.ToArray();

            foreach (var i in indices)
            {
                byte[] frame = frames[i];
                lamp.PushFrame(frame, i);
            }

            var requestPacket = new MissingFramesRequestPacket();
            client.SendPacket(lamp, requestPacket);

            bool responseReceived = false;
            long[] missing = null;

            NetUtils.VoyagerClient.onReceived += OnReceived;

            void OnReceived(object sender, byte[] data)
            {
                try
                {
                    var packet = Packet.Deserialize<MissingFramesResponsePacket>(data);
                    missing = GetExistingIndices(lamp, packet.indices);
                    responseReceived = true;
                    NetUtils.VoyagerClient.onReceived -= OnReceived;
                }
                catch (System.Exception) { }
            }

            while (!responseReceived)
            {
                var packet = new MissingFramesRequestPacket();
                client.SendPacket(lamp, packet);
                yield return new WaitForSeconds(0.2f);
            }

            var missingFrames = new Dictionary<long, byte[]>();
            foreach (var i in missing)
                missingFrames.Add(i, frames[i]);

            if (missing.Length == 0)
                SendingToLampFinished();
            else
                behaviour.StartCoroutine(SendFramesToLamp(lamp, missingFrames));
        }

        long[] GetExistingIndices(Lamp lamp, long[] indices)
        {
            List<long> existing = new List<long>(indices);
            for (long i = 0; i < indices.Length; i++)
            {
                if (lamp.buffer.FrameExists(indices[i]))
                    existing.Add(indices[i]);
            }
            return existing.ToArray();
        }

        void SendingToLampFinished()
        {
            finished++;
            loadingProcess.UpdateProgress((float)finished / startCount);
            StartSendingToNextLamp();
        }
    }
}

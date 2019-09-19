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
        int doneCount;
        int finished;
        Queue<Lamp> lamps;
        MonoBehaviour behaviour;

        public ProjectBufferSender(Lamp[] lamps, MonoBehaviour behaviour)
        {
            startCount = lamps.Length;
            this.lamps = new Queue<Lamp>(lamps);
            this.behaviour = behaviour;
        }

        public void StartSending()
        {
            StartSendingToNextLamp();
        }

        void StartSendingToNextLamp()
        {
            if (lamps.Count > 0)
            {
                Lamp lamp = lamps.Dequeue();
                doneCount++;
                if (lamp.connected)
                    SendBufferToLamp(lamp);
                else
                    SendingToLampFinished();
            }
        }

        void SendBufferToLamp(Lamp lamp)
        {
            var loading = LoadingBar.CreateLoadProcess($"SENDING BUFFER TO {lamp.serial} ({doneCount}/{startCount})");

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
            lamp.SetItshe(lamp.itshe);

            behaviour.StartCoroutine(SendFramesToLamp(lamp, frames, loading));
        }

        IEnumerator SendFramesToLamp(Lamp lamp, Dictionary<long, byte[]> frames, LoadingBarProcess loading)
        {
            var client = NetUtils.VoyagerClient;
            var indices = frames.Keys.ToArray();

            // SEND FRAMES
            foreach (var i in indices)
                lamp.PushFrame(frames[i], i);

            // REQUEST MISSING FRAMES
            long[] missing = null;
            NetUtils.VoyagerClient.onReceived += OnReceived;

            bool responseReceived = false;
            while (!responseReceived)
            {
                var packet = new MissingFramesRequestPacket();
                client.SendPacket(lamp, packet);
                yield return new WaitForSeconds(0.3f);
            }

            // RECEIVE MISSING FRAMES
            void OnReceived(object sender, byte[] data)
            {
                try
                {
                    var packet = Packet.Deserialize<MissingFramesResponsePacket>(data);
                    missing = GetExistingIndices(lamp, packet.indices);
                    NetUtils.VoyagerClient.onReceived -= OnReceived;
                    responseReceived = true;
                }
                catch (System.Exception) { }
            }


            // PUT TOGETHER MISSING FRAMES
            var missingFrames = new Dictionary<long, byte[]>();
            foreach (var i in missing)
                missingFrames.Add(i, lamp.buffer.GetFrame(i));

            // SEND MISSING FRAMES AGAIN, IF ANY
            if (missing.Length == 0)
            {
                loading.UpdateProgress(1.0f);
                SendingToLampFinished();
            }
            else
            {
                float process = (float)(lamp.buffer.frames - missingFrames.Count) / lamp.buffer.frames;
                loading.UpdateProgress(process);
                behaviour.StartCoroutine(SendFramesToLamp(lamp, missingFrames, loading));
            }
        }

        long[] GetExistingIndices(Lamp lamp, long[] indices)
        {
            List<long> existing = new List<long>();
            foreach (var index in indices)
            {
                if (lamp.buffer.FrameExists(index))
                    existing.Add(index);
            }
            return existing.ToArray();
        }

        void SendingToLampFinished()
        {
            StartSendingToNextLamp();
        }
    }
}
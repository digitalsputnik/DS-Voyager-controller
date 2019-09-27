using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using VoyagerApp.Lamps;
using VoyagerApp.Networking.Packages;
using VoyagerApp.Networking.Packages.Voyager;
using VoyagerApp.UI.Overlays;
using VoyagerApp.Utilities;
using VoyagerApp.Videos;

namespace VoyagerApp.Projects
{
    public class ProjectBufferSender
    {
        int startCount;
        int doneCount;
        int finished;
        int processing;

        List<Lamp> lamps;
        MonoBehaviour behaviour;

        LoadingBarProcess loading;

        double time;
        long framesAll;
        Dictionary<Lamp, long> framesSent = new Dictionary<Lamp, long>();

        long allFramesSent
        {
            get
            {
                long sent = 0;
                foreach (var val in framesSent.Values)
                    sent += val;
                return sent;
            }
        }

        public ProjectBufferSender(Lamp[] lamps, MonoBehaviour behaviour)
        {
            startCount = lamps.Length;
            this.lamps = new List<Lamp>(lamps);
            this.behaviour = behaviour;
        }

        public void StartSending()
        {
            loading = LoadingBar.CreateLoadProcess($"SENDING BUFFER TO LAMPS");
            loading.onCancel = Cancel;
            time = TimeUtils.Epoch + NetUtils.VoyagerClient.TimeOffset;
            foreach (var lamp in lamps)
            {
                if (lamp.connected)
                {
                    framesAll += lamp.buffer.ExistingFramesCount;
                    Video video = lamp.video;

                    var start = video.lastStartTime + NetUtils.VoyagerClient.TimeOffset;
                    var packet = new PacketCollection(
                        new SetVideoPacket(video.frames, start),
                        new SetFpsPacket((int)video.fps),
                        new SetItshePacket(lamp.itshe)
                    );
                    NetUtils.VoyagerClient.SendPacket(lamp, packet, time);
                }
            }

            StartSendingToNextLamp();
        }

        void StartSendingToNextLamp()
        {
            if (lamps.Count > 0)
            {
                processing++;
                Lamp lamp = lamps[processing & (lamps.Count - 1)];
                if (lamp.connected)
                    SendBufferToLamp(lamp);
                else
                    SendingToLampFinished(lamp);
            }
            else
            {
                loading.UpdateProgress(1.0f);
                Cancel();
            }
        }

        void SendBufferToLamp(Lamp lamp)
        {
            behaviour.StartCoroutine(SendFramesToLamp(lamp));
        }

        IEnumerator SendFramesToLamp(Lamp lamp)
        {
            Debug.Log($"{lamp.serial} Started sending buffer");
            var client = NetUtils.VoyagerClient;

            // REQUEST MISSING FRAMES
            long[] missing = new long[0];
            NetUtils.VoyagerClient.onReceived += OnReceived;

            bool timeout = false;
            float startTime = Time.time;
            bool responseReceived = false;

            // RECEIVE MISSING FRAMES
            void OnReceived(object sender, byte[] data)
            {
                try
                {
                    IPEndPoint endpoint = (IPEndPoint)sender;
                    if (endpoint.Address.ToString() == lamp.address.ToString())
                    {
                        var packet = Packet.Deserialize<MissingFramesResponsePacket>(data);
                        missing = GetExistingIndices(lamp, packet.indices);
                        responseReceived = true;

                        framesSent[lamp] = lamp.buffer.ExistingFramesCount - missing.Length;
                        loading.UpdateProgress((float)allFramesSent / framesAll);
                    }
                }
                catch (System.Exception) { }
            }

            var sendPacket = new MissingFramesRequestPacket();
            client.SendPacket(lamp, sendPacket);

            while(!responseReceived && !timeout)
            {
                float passed = Time.time - startTime;
                timeout = passed >= 3.0f;
                yield return new WaitForSeconds(0.1f);
            }

            NetUtils.VoyagerClient.onReceived -= OnReceived;

            if (responseReceived)
            {
                Debug.Log($"{lamp.serial} here!");

                long sent = 0;

                foreach (var index in missing)
                {
                    var packet = new SetFramePacket(index, lamp.buffer.GetFrame(index));
                    NetUtils.VoyagerClient.SendPacketToVideoPort(lamp, packet, time);
                    framesSent[lamp] = lamp.buffer.ExistingFramesCount - missing.Length + sent;
                    loading.UpdateProgress((float)allFramesSent / framesAll);
                    yield return new WaitForSeconds(0.01f);
                    sent++;
                }

                if (missing.Length == 0)
                    SendingToLampFinished(lamp);

                yield return new WaitForSeconds(0.1f);
            }

            // MOVE TO NEXT
            StartSendingToNextLamp();
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

        void SendingToLampFinished(Lamp lamp)
        {
            lamps.Remove(lamp);
            StartSendingToNextLamp();
        }

        public void Cancel()
        {
            behaviour.StopAllCoroutines();
        }
    }
}
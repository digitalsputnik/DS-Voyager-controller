using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using VoyagerApp.Networking.Voyager;
using VoyagerApp.Utilities;

namespace VoyagerApp.Projects
{
    public class ProjectLoadBuffer
    {
        const float FRAMES_SLEEP = 0.02f;
        const double TIMEOUT = 3.0f;

        List<Lamps.Lamp> lamps;
        double time;

        Action<float> onProgress;
        long totalFrameCount;
        long totalFramesSent;

        public ProjectLoadBuffer(List<Lamps.Lamp> lamps, Action<float> onProgress)
        {
            this.lamps = lamps;
            this.onProgress = onProgress;
        }

        public void StartSending()
        {
            Task.Run(StartSendingAsync);
            //new Thread(() => { Task.Run(StartSendingAsync); }).Start();
        }

        async Task StartSendingAsync()
        {
            List<Lamps.Lamp> connected = FilterConnectedLamps(lamps);
            totalFrameCount = GetExistingFrameCount(connected);
            time = TimeUtils.Epoch + NetUtils.VoyagerClient.TimeOffset;

            connected.ForEach(SendVideoMetadata);

            bool finished = false;

            do {
                finished = await SendMissingFramesAsync(connected);
                await Task.Delay((int)(FRAMES_SLEEP * 1000));
            }
            while (!finished);

            MainThread.Dispach(() => { onProgress?.Invoke(1.0f); });
        }

        void SendVideoMetadata(Lamps.Lamp lamp)
        {
            Videos.Video video = lamp.video;
            var start = video.lastStartTime + NetUtils.VoyagerClient.TimeOffset;
            var packet = new PacketCollection(
                new SetVideoPacket(video.frames, start),
                new SetFpsPacket(video.fps),
                new SetItshePacket(lamp.itshe)
            );
            for (int i = 0; i < 5; i++)
                NetUtils.VoyagerClient.SendPacket(lamp, packet, VoyagerClient.PORT_SETTINGS, time);
        }

        void SendFrameToLamp(Lamps.Lamp lamp, long index)
        {
            var packet = new SetFramePacket(index, lamp.itshe, lamp.buffer.GetFrame(index));
            NetUtils.VoyagerClient.SendPacket(lamp, packet, VoyagerClient.PORT_VIDEO, time);
        }

        void UpdateProgress(long tempSent)
        {
            long sent = 0;
            sent = totalFramesSent + (long)(tempSent * 0.7f);
            float progress = (float)sent / totalFrameCount;
            MainThread.Dispach(() => { onProgress?.Invoke(progress); });
        }

        async Task<bool> SendMissingFramesAsync(List<Lamps.Lamp> connected)
        {
            var missingFramesPerLamp = await FetchMissingFramesAsync(connected);

            long tempSent = 0;

            while (missingFramesPerLamp.Count != 0)
            {
                for (int i = 0; i < missingFramesPerLamp.Count;)
                {
                    var pair = missingFramesPerLamp[i];

                    if (pair.Item2.Count == 0)
                        missingFramesPerLamp.Remove(pair);
                    else
                    {
                        long index = pair.Item2[0];
                        SendFrameToLamp(pair.Item1, index);
                        missingFramesPerLamp[i].Item2.Remove(index);
                        UpdateProgress(tempSent++);
                        i++;
                    }
                }

                await Task.Delay((int)(FRAMES_SLEEP * 1000));
            }

            missingFramesPerLamp = await FetchMissingFramesAsync(connected);

            long approvedFrames = totalFrameCount;

            foreach (var pair in missingFramesPerLamp)
                approvedFrames -= pair.Item2.Count;

            totalFramesSent = approvedFrames;

            return missingFramesPerLamp.Count == 0;
        }

        static List<Lamps.Lamp> FilterConnectedLamps(List<Lamps.Lamp> lamps)
        {
            return lamps.Where(lamp => lamp.connected && lamp.video != null).ToList();
        }

        static async Task<List<(Lamps.Lamp, List<long>)>> FetchMissingFramesAsync(List<Lamps.Lamp> lamps)
        {
            List<Task<(Lamps.Lamp, long[])>> tasks = new List<Task<(Lamps.Lamp, long[])>>();

            foreach (var lamp in lamps)
                tasks.Add(FetchMissingFramesAsync(lamp));

            await Task.WhenAll(tasks);

            List<(Lamps.Lamp, List<long>)> results = new List<(Lamps.Lamp, List<long>)>();

            foreach (var task in tasks)
            {
                if (task.Result.Item2 != null)
                    if (task.Result.Item2.Length != 0)
                        results.Add((task.Result.Item1, task.Result.Item2.ToList()));
            }

            return results;
        }

        static async Task<(Lamps.Lamp, long[])> FetchMissingFramesAsync(Lamps.Lamp lamp)
        {
            bool received = false;
            long[] missing = missing = new long[0];

            lamp.OnDataReceived += DataReceived;
            NetUtils.VoyagerClient.SendPacket(lamp, new MissingFramesRequestPacket(), VoyagerClient.PORT_SETTINGS);

            void DataReceived(byte[] data)
            {
                var packet = Packet.Deserialize<MissingFramesResponsePacket>(data);
                if (packet != null && packet.op == OpCode.MissingFramesResponse)
                {
                    missing = packet.indices;
                    received = true;
                }
            }

            double startTime = TimeUtils.Epoch;

            while (!received)
            {
                if ((TimeUtils.Epoch - startTime) < TIMEOUT)
                    await Task.Delay(2);
                else
                {
                    Debug.LogError("TIMEOUT from " + lamp.serial);
                    lamp.OnDataReceived -= DataReceived;
                    return (lamp, null);
                }
            }

            lamp.OnDataReceived -= DataReceived;
            return (lamp, FilterMissingFrames(lamp, missing));
        }

        static long GetExistingFrameCount(List<Lamps.Lamp> lamps)
        {
            long available = 0;
            foreach (var lamp in lamps)
                available += lamp.buffer.ExistingFramesCount;
            return available;
        }

        static long GetAllFrameCount(List<Lamps.Lamp> lamps)
        {
            long available = 0;
            foreach (var lamp in lamps)
                available += lamp.buffer.frames;
            return available;
        }

        static long[] FilterMissingFrames(Lamps.Lamp lamp, long[] indices)
        {
            List<long> existing = new List<long>();
            foreach (var index in indices)
            {
                if (lamp.buffer.FrameExists(index))
                    existing.Add(index);
            }
            return existing.ToArray();
        }
    }
}
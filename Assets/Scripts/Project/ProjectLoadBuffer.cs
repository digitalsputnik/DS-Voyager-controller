using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VoyagerApp.Lamps;
using VoyagerApp.Networking.Packages;
using VoyagerApp.Networking.Packages.Voyager;
using VoyagerApp.Utilities;
using VoyagerApp.Videos;

namespace VoyagerApp.Projects
{
    public class ProjectLoadBuffer
    {
        const float FRAMES_SLEEP = 0.025f;
        const double TIMEOUT = 3.0f;

        List<Lamp> lamps;
        bool overwriteTime;
        double time;

        Action<float> onProgress;
        long totalFrameCount;
        long totalFramesSent;

        public ProjectLoadBuffer(List<Lamp> lamps, Action<float> onProgress)
        {
            this.lamps = lamps;
            this.onProgress = onProgress;
        }

        public void StartSending()
        {
            StartSending(true);
        }

        public void StartSending(bool overwriteTime)
        {
            this.overwriteTime = overwriteTime;
            new Thread(() => { Task.Run(StartSendingAsync); }).Start();
        }

        async Task StartSendingAsync()
        {
            List<Lamp> connected = FilterConnectedLamps(lamps);

            totalFrameCount = overwriteTime ?
                GetExistingFrameCount(connected) :
                GetAllFrameCount(connected);

            time = TimeUtils.Epoch + NetUtils.VoyagerClient.TimeOffset;

            if (overwriteTime)
                connected.ForEach(SendVideoMetadata);

            bool finished = false;

            do {
                finished = await SendMissingFramesAsync(connected);
                await Task.Delay((int)(FRAMES_SLEEP * 1000));
            }
            while (!finished);

            MainThread.Dispach(() => { onProgress?.Invoke(1.0f); });
        }

        void SendVideoMetadata(Lamp lamp)
        {
            Video video = lamp.video;
            if (!overwriteTime) time = video.lastTimestamp;
            var start = video.lastStartTime + NetUtils.VoyagerClient.TimeOffset;
            var packet = new PacketCollection(
                new SetVideoPacket(video.frames, start),
                new SetFpsPacket((int)video.fps),
                new SetItshePacket(lamp.itshe)
            );
            NetUtils.VoyagerClient.SendPacket(lamp, packet, time);
        }

        void SendFrameToLamp(Lamp lamp, long index)
        {
            var packet = new SetFramePacket(index, lamp.buffer.GetFrame(index));
            NetUtils.VoyagerClient.SendPacketToVideoPort(lamp, packet, time);
        }

        void UpdateProgress(long tempSent)
        {
            long sent = 0;
            if (overwriteTime)
                sent = totalFramesSent + (long)(tempSent * 0.7f);
            else
                sent = totalFramesSent;
            float progress = (float)sent / totalFrameCount;
            MainThread.Dispach(() => { onProgress?.Invoke(progress); });
        }

        async Task<bool> SendMissingFramesAsync(List<Lamp> connected)
        {
            var missingFramesPerLamp = await FetchMissingFramesAsync(connected);

            long tempSent = 0;

            while (missingFramesPerLamp.Count != 0)
            {
                for (int i = 0; i < missingFramesPerLamp.Count;)
                {
                    var pair = missingFramesPerLamp[i];
                    if (!overwriteTime) time = pair.Item1.lastTimestamp;

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

        static List<Lamp> FilterConnectedLamps(List<Lamp> lamps)
        {
            return lamps.Where(lamp => lamp.connected).ToList();
        }

        static async Task<List<(Lamp, List<long>)>> FetchMissingFramesAsync(List<Lamp> lamps)
        {
            List<Task<(Lamp, long[])>> tasks = new List<Task<(Lamp, long[])>>();

            foreach (var lamp in lamps)
                tasks.Add(FetchMissingFramesAsync(lamp));

            await Task.WhenAll(tasks);

            List<(Lamp, List<long>)> results = new List<(Lamp, List<long>)>();

            foreach (var task in tasks)
            {
                if (task.Result.Item2 != null)
                    if (task.Result.Item2.Length != 0)
                        results.Add((task.Result.Item1, task.Result.Item2.ToList()));
            }

            return results;
        }

        static async Task<(Lamp, long[])> FetchMissingFramesAsync(Lamp lamp)
        {
            bool received = false;
            long[] missing = null;

            lamp.OnDataReceived += DataReceived;
            NetUtils.VoyagerClient.SendPacket(lamp, new MissingFramesRequestPacket());

            void DataReceived(byte[] data)
            {
                var packet = Packet.Deserialize<MissingFramesResponsePacket>(data);
                if (packet != null)
                {
                    missing = packet.indices;
                    received = true;
                    lamp.OnDataReceived -= DataReceived;
                }
            }

            double startTime = TimeUtils.Epoch;

            while (!received && (TimeUtils.Epoch - startTime) < TIMEOUT) await Task.Delay(2);

            if (missing == null) return (lamp, null);

            return (lamp, FilterMissingFrames(lamp, missing));
        }

        static long GetExistingFrameCount(List<Lamp> lamps)
        {
            long available = 0;
            foreach (var lamp in lamps)
                available += lamp.buffer.ExistingFramesCount;
            return available;
        }

        static long GetAllFrameCount(List<Lamp> lamps)
        {
            long available = 0;
            foreach (var lamp in lamps)
                available += lamp.buffer.frames;
            return available;
        }

        static long[] FilterMissingFrames(Lamp lamp, long[] indices)
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
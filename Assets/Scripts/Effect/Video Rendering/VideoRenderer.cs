﻿using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Video;
using VoyagerApp.Effects;
using VoyagerApp.Lamps;
using VoyagerApp.UI;
using VoyagerApp.Utilities;

namespace VoyagerApp.Videos
{
    public class VideoRenderer : MonoBehaviour
    {
        #region Singleton
        internal static VideoRenderer instance;
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
                Destroy(gameObject);
        }
        #endregion

        public static event VideoRenderingProgressHandler onProgressChanged;
        public static event VideoRenderStateHandler onStateChanged;
        public static float Progress => instance.prevProgress;

        public static RenderState state { get; private set; }

        [SerializeField] internal Material renderMaterial = null;

        VideoPlayer videoPlayer;
        RenderState prevState = null;
        RenderTexture renderTexture;

        float prevProgress = 1.0f;
        bool lampEventsSubscribed;

        public static void SetState(RenderState state)
        {
            instance.prevState = VideoRenderer.state;
            VideoRenderer.state = state;
        }

        void Start()
        {
            videoPlayer = GetComponent<VideoPlayer>();

            videoPlayer.started          += Started;
            videoPlayer.prepareCompleted += Prepered;
            videoPlayer.seekCompleted    += SeekComplited;

            state = new DoneState();

            ApplicationState.OnNewProject              += HandleApplicationStateInterupt;
            NetUtils.VoyagerClient.onConnectionChanged += OnConnectionChanged;

            SubscribeLampEvents();
        }

        void SubscribeLampEvents()
        {
            if (!lampEventsSubscribed)
            {
                LampManager.instance.onLampEffectChanged += HandleLampInterupt;
                LampManager.instance.onLampMappingChanged += HandleLampInterupt;
                LampManager.instance.onLampItsheChanged += HandleLampItsheInterupt;
                lampEventsSubscribed = true;
            }
        }

        void UnsubscribeLampEvents()
        {
            if (lampEventsSubscribed)
            {
                LampManager.instance.onLampEffectChanged -= HandleLampInterupt;
                LampManager.instance.onLampMappingChanged -= HandleLampInterupt;
                LampManager.instance.onLampItsheChanged -= HandleLampItsheInterupt;
                lampEventsSubscribed = false;
            }
        }

        void OnConnectionChanged()             => Interupt();
        void HandleApplicationStateInterupt()  => Interupt();
        void Started(VideoPlayer source)       => RaiseVideoEvent(VideoRenderEvent.Starting);
        void Prepered(VideoPlayer source)      => RaiseVideoEvent(VideoRenderEvent.Prepared);
        void SeekComplited(VideoPlayer source) => RaiseVideoEvent(VideoRenderEvent.Seeked);

        void Update()
        {
            state = state.Update();

            if (state != prevState)
            {
                onStateChanged?.Invoke(state);
                prevState = state;
            }
        }

        void RaiseVideoEvent(VideoRenderEvent e)
        {
            state.HandleEvent(e);
        }

        void HandleLampInterupt(Lamp lamp)
        {
            if (WorkspaceUtils.Lamps.Contains(lamp))
                Interupt();
        }

        void HandleLampItsheInterupt(Lamp lamp)
        {
            // TODO: Also add resend buffer state here.
            if (WorkspaceUtils.SelectedLamps.All(l => l.effect is Video && l.buffer.rendered) && (state is DoneState || state is ResendBufferState))
            {
                UnsubscribeLampEvents();
                if (state is ResendBufferState resendState)
                    resendState.AddLamp(lamp);
                else
                {
                    state = new ResendBufferState();
                    ((ResendBufferState)state).AddLamp(lamp);
                }
                SubscribeLampEvents();
            }
            else
                Interupt();
        }

        void Interupt()
        {
            Clear();
            state.OnCancel();
            state = new PrepereQueueState();
        }

        #region Internal Controls

        internal static RenderTexture VideoTexture => instance.renderTexture;
        internal static long CurrentFrameIndex => instance.videoPlayer.frame;
        internal static long FrameCount => (long)instance.videoPlayer.frameCount;

        internal static void SetVideo(Video video)
        {
            instance.renderTexture = new RenderTexture(
                (int)video.width,
                (int)video.height,
                1, RenderTextureFormat.ARGB32);
            instance.renderTexture.Create();

            instance.videoPlayer.url = video.path;
            instance.videoPlayer.targetTexture = instance.renderTexture;
            instance.videoPlayer.Prepare();
        }

        internal static void Play()
        {
            instance.videoPlayer.Play();
        }

        internal static void Pause()
        {
            instance.videoPlayer.Pause();
        }

        internal static void Seek(long frame)
        {
            instance.videoPlayer.frame = frame;
        }

        internal static void SetFrameRate(int fps)
        {
            float rate = 1.0f / instance.videoPlayer.frameRate * fps;
            instance.videoPlayer.playbackSpeed = rate;
        }

        internal static void UpdateProgress(float progress)
        {
            if (math.abs(progress - instance.prevProgress) > 0.001f)
            {
                onProgressChanged?.Invoke(progress);
                instance.prevProgress = progress;
            }
        }

        internal static void Clear()
        {
            instance.videoPlayer.Stop();
        }

        #endregion
    }

    public enum VideoRenderEvent
    {
        Prepared,
        Starting,
        Seeked,
    }

    public delegate void VideoRenderingProgressHandler(float progress);
    public delegate void VideoRenderStateHandler(RenderState state);
}
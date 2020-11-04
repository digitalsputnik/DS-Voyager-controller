﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DigitalSputnik.Videos;
using DigitalSputnik.Voyager;
using UnityEngine;
using UnityEngine.Video;
using VoyagerController.Effects;
using VoyagerController.Workspace;

namespace VoyagerController.Rendering
{
    [RequireComponent(typeof(VideoPlayer))]
    public class VideoEffectRenderer : MonoBehaviour
    {
        private static VideoEffectRenderer _instance;
        private void Awake() => _instance = this;

        public static VideoPlayer VideoPlayer => _instance._videoPlayer;
        public static RenderTexture RenderTexture => _instance._renderTexture;
        
        private VideoRenderState _state = new IdleState();
        private VideoPlayer _videoPlayer = null;
        private RenderTexture _renderTexture = null;
        
        private Dictionary<VoyagerLamp, Effect> _prevEffects = new Dictionary<VoyagerLamp, Effect>(); 

        private void Start()
        {
            _videoPlayer = GetComponent<VideoPlayer>();
        }

        private void Update()
        {
            var current = _state;
            
            if (EffectsChanged())
                _state = new PrepareState();
            
            _state = _state.Update();
            
            if (_state != current) Debugger.LogInfo($"Render state changed to {_state}");
        }

        public static void PrepareVideoPlayer(Video video, Action prepared)
        {
            _instance.StopAllCoroutines();
            
            if (_instance._renderTexture != null)
                Destroy(_instance._renderTexture);
            
            _instance._renderTexture = CreateRenderer(video.Width, video.Height);
            
            VideoPlayer.url = video.Path;
            VideoPlayer.renderMode = VideoRenderMode.RenderTexture;
            VideoPlayer.targetTexture = RenderTexture;
            VideoPlayer.isLooping = true;
            VideoPlayer.Prepare();

            _instance.StartCoroutine(WaitUntilVideoPlayerPrepared(prepared));
        }

        private static IEnumerator WaitUntilVideoPlayerPrepared(Action prepared)
        {
            yield return new WaitUntil(() => VideoPlayer.isPrepared);
            prepared?.Invoke();
        }

        private static RenderTexture CreateRenderer(int width, int height)
        {
            var render = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
            render.Create();
            return render;
        }
        
        internal static void Clear()
        {
            VideoPlayer.Stop();
        }

        public static void Stop()
        {
            _instance._state = new IdleState();
            Clear();
        }

        private bool EffectsChanged()
        {
            var result = false;
            var lamps = WorkspaceManager.GetItems<VoyagerItem>()
                .Select(i => i.LampHandle)
                .Where(v => Metadata.Get(v.Serial).Effect is VideoEffect)
                .ToArray();

            if (!lamps.All(l => _prevEffects.ContainsKey(l)))
                result = true;

            if (!result && lamps.Any(l => Metadata.Get(l.Serial).Effect != _prevEffects[l]))
                result = true;
            
            _prevEffects.Clear();
            foreach (var lamp in lamps)
                _prevEffects.Add(lamp, Metadata.Get(lamp.Serial).Effect);

            return result;
        }
    }
}

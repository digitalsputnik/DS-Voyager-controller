using System;
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

        [SerializeField] private Material _material;
        
        private VideoRenderState _state = new IdleState();
        private VideoPlayer _videoPlayer = null;
        private RenderTexture _renderTexture = null;
        
        private readonly Dictionary<VoyagerLamp, Effect> _prevEffects = new Dictionary<VoyagerLamp, Effect>();
        private readonly Dictionary<VoyagerLamp, double> _prevEffectTimes = new Dictionary<VoyagerLamp, double>();
        private bool _effectModified = false;

        private void Start()
        {
            _videoPlayer = GetComponent<VideoPlayer>();
            EffectManager.OnEffectModified += OnEffectModified;
            SelectionMove.SelectionMoveEnded += SelectionMoveEnded;
        }

        private void OnDestroy()
        {
            EffectManager.OnEffectModified -= OnEffectModified;
        }

        private void OnEffectModified(Effect effect)
        {
            if (_prevEffects.ContainsValue(effect))
                _effectModified = true;
        }
        
        private void SelectionMoveEnded()
        {
            if (WorkspaceSelection
                .GetSelected<VoyagerItem>()
                .Select(v => v.LampHandle)
                .Any(l => _prevEffects.ContainsKey(l)))
            {
                _effectModified = true;
            }
        }

        private void Update()
        {
            var current = _state;
            
            if (EffectsChanged())
                _state = new PrepareState();
            
            _state = _state.Update();
            
            if (_state != current) Debugger.LogInfo($"Render state changed to {_state}");
        }
        
        public static Texture2D GetFrameTexture2D(Effect effect)
        {
            var render = new RenderTexture(RenderTexture.descriptor);
            ShaderUtils.ApplyEffectToMaterial(_instance._material, effect);
            var prevActive = RenderTexture.active;
            Graphics.Blit(RenderTexture, render, _instance._material);
            var texture = render.ToTexture2D();
            RenderTexture.active = prevActive;
            Destroy(render);
            return texture;
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
                .Where(v => Metadata.Get<LampData>(v.Serial).Effect is VideoEffect)
                .ToArray();

            if (_effectModified)
            {
                result = true;
                _effectModified = false;
            }
            
            if (!lamps.All(l => _prevEffects.ContainsKey(l)))
                result = true;

            if (!result && lamps.Any(l => Metadata.Get<LampData>(l.Serial).Effect != _prevEffects[l]))
                result = true;
            
            if (!lamps.All(l => _prevEffectTimes.ContainsKey(l)))
                result = true;

            if (!result && lamps.Any(l =>
                Math.Abs(Metadata.Get<LampData>(l.Serial).TimeEffectApplied - _prevEffectTimes[l]) > 0.00001))
                result = true;
            
            _prevEffects.Clear();
            _prevEffectTimes.Clear();

            foreach (var lamp in lamps)
            {
                _prevEffects.Add(lamp, Metadata.Get<LampData>(lamp.Serial).Effect);
                _prevEffectTimes.Add(lamp,Metadata.Get<LampData>(lamp.Serial).TimeEffectApplied);
            }

            return result;
        }
    }
}

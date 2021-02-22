using System.Linq;
using Klak.Syphon;
using UnityEngine;
using VoyagerController.Effects;
using VoyagerController.Workspace;

namespace VoyagerController.Rendering
{
    [RequireComponent(typeof(SyphonClient))]
    public class SyphonRenderer : MonoBehaviour
    {
        public static RenderTexture SyphonRenderTexture { get; private set; }

        [SerializeField] private Material _material;
        
        private RenderTexture _render;
        private SyphonClient _client;
        private SyphonEffect _effect;
        private bool _streaming;

        private void Start()
        {
            if (Application.platform != RuntimePlatform.OSXEditor &&
                Application.platform != RuntimePlatform.OSXPlayer)
            {
                Destroy(gameObject);
                return;
            }

            _render = new RenderTexture(640, 480,  0, RenderTextureFormat.ARGB32);
            _render.Create();

            SyphonRenderTexture = _render;

            _effect = EffectManager.GetEffects<SyphonEffect>().First();
            
            _client = GetComponent<SyphonClient>();
            _client.targetTexture = _render;
            
            UpdateEffectConnection(_effect);

            EffectManager.OnEffectModified += UpdateEffectConnection;
        }

        private void OnDestroy()
        {
            EffectManager.OnEffectModified -= UpdateEffectConnection;
        }

        private void UpdateEffectConnection(Effect effect)
        {
            _client.serverName = _effect.Server.Server;
            _client.appName = _effect.Server.Application;
        }

        private void Update()
        {
            if (!_streaming && AnyLampIsStreaming)
                SetupStreaming();
            
            if (_streaming && !AnyLampIsStreaming)
                EndStreaming();
            
            if (_streaming) RenderStream();
        }

        private void SetupStreaming()
        {
            _client.enabled = true;
            _streaming = true;
        }

        private void EndStreaming()
        {
            _client.enabled = false;
            _streaming = false;
        }

        private void RenderStream()
        {
            var frame = GetFrameTexture(_effect);
            var delay = _effect.Delay;
            
            foreach (var item in WorkspaceManager.GetItems<VoyagerItem>())
            {
                var voyager = item.LampHandle;
                var coords = TextureExtensions.MapLampToVideoCoords(voyager, frame);
                var rgb =  TextureExtensions.CoordsToColors(coords.ToArray(), frame).ToRgbArray();
                LampEffectsWorker.ApplyStreamFrameToVoyager(voyager, rgb, delay);
            }
            
            Destroy(frame);
        }

        private Texture2D GetFrameTexture(Effect effect)
        {
            var render = new RenderTexture(_render);
            ShaderUtils.ApplyEffectToMaterial(_material, effect);
            var prevActive = RenderTexture.active;
            Graphics.Blit(_render, render, _material);
            var texture = render.ToTexture2D();
            RenderTexture.active = prevActive;
            Destroy(render);
            return texture;
        }

        private static bool AnyLampIsStreaming => WorkspaceManager
            .GetItems<VoyagerItem>()
            .Any(v => Metadata.GetLamp(v.LampHandle.Serial).Effect is SyphonEffect);
    }
}
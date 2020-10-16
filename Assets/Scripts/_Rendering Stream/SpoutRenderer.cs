using System.Linq;
using Klak.Spout;
using UnityEngine;
using VoyagerController.Effects;
using VoyagerController.Workspace;

namespace VoyagerController.Rendering
{
    [RequireComponent(typeof(SpoutReceiver))]
    public class SpoutRenderer : MonoBehaviour
    {
        private RenderTexture _render;
        private SpoutReceiver _client;
        private SpoutEffect _effect;
        private bool _streaming;

        private void Start()
        {
            if (Application.platform != RuntimePlatform.WindowsPlayer &&
                Application.platform != RuntimePlatform.WindowsEditor)
            {
                Destroy(gameObject);
                return;
            }

            _render = new RenderTexture(640, 480,  0, RenderTextureFormat.ARGB32);
            _render.Create();

            _effect = EffectManager.GetEffects<SpoutEffect>().First();
            
            _client = GetComponent<SpoutReceiver>();
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
            _client.sourceName = _effect.Source;
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
            var frame = _render.ToTexture2D();
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

        private static bool AnyLampIsStreaming => WorkspaceManager
            .GetItems<VoyagerItem>()
            .Any(v => Metadata.Get(v.LampHandle.Serial).Effect is SyphonEffect);
    }
}
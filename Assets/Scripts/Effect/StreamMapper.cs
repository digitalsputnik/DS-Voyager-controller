using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Klak.Spout;
using Klak.Syphon;
using UnityEngine;
using VoyagerApp.Lamps.Voyager;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.Effects
{
    public class StreamMapper : MonoBehaviour
    {
        [SerializeField] MeshRenderer meshRenderer = null;
        [SerializeField] Material renderMaterial = null;
        SyphonClient syphon = null;
        SpoutReceiver spout = null;
        RenderTexture render;

        Effect effect;

        public int Delay { get; set; } = 1;
        public int Fps { get; set; } = 30;

        List<(VoyagerLamp, byte[])> streamBuffer = new List<(VoyagerLamp, byte[])>();
        double time;

        void Awake()
        {
            syphon = GetComponent<SyphonClient>();
            spout = GetComponent<SpoutReceiver>();
            meshRenderer = GetComponent<MeshRenderer>();

            render = new RenderTexture(1280, 720, 24);
            syphon.targetTexture = render;
            spout.targetTexture = render;
            meshRenderer.material.SetTexture("_MainTex", render);

            SelectionMove.onSelectionMoveEnded += SelectionMoved;
        }

        void OnDestroy()
        {
            SelectionMove.onSelectionMoveEnded -= SelectionMoved;
        }

        public void SetEffect(Effect effect)
        {
            this.effect = effect;

            if (effect is SyphonStream syphonStream)
            {
                syphon.serverName = syphonStream.server;
                syphon.appName = syphonStream.application;
                syphon.enabled = true;
                spout.enabled = false;
            }

            if (effect is SpoutStream spoutStream)
            {
                spout.sourceName = spoutStream.source;
                spout.enabled = true;
                syphon.enabled = false;
                StopAllCoroutines();
                StartCoroutine(RevokeSpoutStream());
            }
        }

        public void UpdateEffectSettings()
        {
            ShaderUtils.ApplyEffectToMaterial(meshRenderer.sharedMaterial, effect);
        }

        float prevRenderTime = 0;
        float prevStreamTime = 0;

        void Update()
        {
            var time = Time.time;
            var renderCap = 1.0f / Fps;
            var streamCap = 1.0f / Fps / 2.0f;

            if ((time - prevRenderTime) >= renderCap)
            {
                RenderFrame();
                prevRenderTime = time;
            }

            if ((time-prevStreamTime) >= streamCap)
            {
                SendFrames();
                prevStreamTime = time;
            }
        }

        IEnumerator RevokeSpoutStream()
        {
            while(gameObject.activeSelf)
            {
                if (effect is SpoutStream spoutEffect)
                {
                    spout.sourceName = "";
                    spout.sourceName = spoutEffect.source;
                }

                yield return new WaitForSeconds(3.0f);
            }
        }

        void RenderFrame()
        {
            var material = renderMaterial;
            var temp = RenderTexture.GetTemporary(render.descriptor);

            ShaderUtils.ApplyEffectToMaterial(material, effect);

            Graphics.Blit(render, temp, material);

            var texture = TextureUtils.RenderTextureToTexture2D(temp);
            StoreFrameBuffer(texture);

            RenderTexture.ReleaseTemporary(temp);
            Destroy(texture);
        }

        void StoreFrameBuffer(Texture2D texture)
        {
            streamBuffer.Clear();
            time = TimeUtils.Epoch + NetUtils.VoyagerClient.TimeOffset + ((float)Delay / 1000);
            foreach (var lamp in WorkspaceUtils.VoyagerLamps)
            {
                var coords = VectorUtils.MapLampToVideoCoords(lamp, texture);
                var colors = TextureUtils.CoordsToColors(coords, texture);
                streamBuffer.Add((lamp, ColorUtils.ColorsToBytes(colors)));
            }
        }

        void SendFrames()
        {
            foreach (var pair in streamBuffer)
            {
                var lamp = pair.Item1;
                var colors = ColorUtils.BytesToColors(pair.Item2);
                lamp.PushStreamFrame(colors, time);
            }
        }

        void SelectionMoved()
        {
            if (gameObject.activeInHierarchy)
            {
                foreach (var selected in WorkspaceUtils.SelectedLampItems)
                    HandleLampMove(selected);
            }
        }

        void HandleLampMove(LampItemView item)
        {
            var mapping = GetLampMapping(item);
            var lamp = item.lamp;

            if (effect != null)
                lamp.SetEffect(effect);

            lamp.SetMapping(mapping);
        }

        EffectMapping GetLampMapping(LampItemView lamp)
        {
            var allPixels = lamp.PixelWorldPositions();
            Vector2[] pixels = {
                    allPixels.First(),
                    allPixels.Last()
            };

            for (int i = 0; i < pixels.Length; i++)
            {
                Vector2 pos = pixels[i];
                Vector2 local = transform.InverseTransformPoint(pos);

                float x = local.x + 0.5f;
                float y = local.y + 0.5f;

                pixels[i] = new Vector2(x, y);
            }

            return new EffectMapping(pixels[0], pixels[1]);
        }

        void OnApplicationFocus(bool focus)
        {
            if (focus && gameObject.activeSelf && effect is SpoutStream)
            {
                spout.sourceName = "";
                SetEffect(this.effect);
            }
        }

        void OnApplicationPause(bool pause)
        {
            if (!pause && gameObject.activeSelf && effect is SpoutStream)
            {
                spout.sourceName = "";
                SetEffect(this.effect);
            }
        }
    }
}
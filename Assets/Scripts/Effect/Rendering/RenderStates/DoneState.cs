﻿using System.Linq;
using System.Threading;
using UnityEngine;
using VoyagerApp.Effects;
using VoyagerApp.Utilities;

namespace VoyagerApp.Videos
{
    public class DoneState : RenderState
    {
        const double CONFIRM_STATE_TIME = 10.0f;

        double _startTime;

        public DoneState()
        {
            _startTime = TimeUtils.Epoch;
        }

        public override RenderState Update()
        {
            if (!WorkspaceUtils.Lamps.Where(l => l.effect is Image).All(l => l.buffer.rendered))
            {
                foreach (var lamp in WorkspaceUtils.Lamps.Where(l => l.effect is Image))
                {
                    var image = (Image)lamp.effect;
                    var texture = image.image;
                    
                    var material = VideoRenderer.instance.renderMaterial;
                    var render = new RenderTexture(texture.width, texture.height, 1, RenderTextureFormat.ARGB32);

                    ShaderUtils.ApplyEffectToMaterial(material, image);

                    var prevActive = RenderTexture.active;
                    Graphics.Blit(texture, render, material);
                    texture = TextureUtils.RenderTextureToTexture2D(render);
                    RenderTexture.active = prevActive;

                    var coords = VectorUtils.MapLampToVideoCoords(lamp, texture);
                    var colors = TextureUtils.CoordsToColors(coords, texture);
                    lamp.PushFrame(colors, 0);
                }
                return new ConfirmPixelsState();
            }

            if (!WorkspaceUtils.Lamps.Where(l => l.effect is Video).All(l => l.buffer.rendered))
                return new PrepereQueueState();

            if (TimeUtils.Epoch - _startTime > CONFIRM_STATE_TIME)
                return new ConfirmPixelsState();

            return this;
        }

        public override void HandleEvent(VideoRenderEvent type) { }
    }
}

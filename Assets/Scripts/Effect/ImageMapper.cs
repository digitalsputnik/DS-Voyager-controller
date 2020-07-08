using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.Effects
{
    public class ImageMapper : MonoBehaviour
    {
        Image _image;
        MeshRenderer _renderMesh;

        void Start()
        {
            _renderMesh = GetComponent<MeshRenderer>();
            SelectionMove.onSelectionMoveEnded += ResetImageEffect;
        }

        void OnDestroy()
        {
            SelectionMove.onSelectionMoveEnded -= ResetImageEffect;
        }

        public void SetImage(Image image)
        {
            _image = image;
            var texture = _image.image;
            _renderMesh.material.SetTexture("_MainTex", texture);
            ResizeMesh(texture);
            ResetImageEffect();
        }

        public void UpdateEffectSettings()
        {
            ShaderUtils.ApplyEffectToMaterial(_renderMesh.sharedMaterial, _image);
        }

        void ResizeMesh(Texture2D texture)
        {
            Vector2 maxScale = CalculateMeshMaxScale();

            float maxScaleAspect = maxScale.x / maxScale.y;
            float videoAspect = (float)texture.width / texture.height;

            Vector2 s = maxScale;

            if (videoAspect > maxScaleAspect)
                s.y = maxScale.y / videoAspect * maxScaleAspect;
            else if (videoAspect < maxScaleAspect)
                s.x = maxScale.x / maxScaleAspect * videoAspect;

            transform.localScale = s;
        }

        Vector2 CalculateMeshMaxScale()
        {
            Vector2 screenWorldSize = VectorUtils.ScreenSizeWorldSpace;
            float width = screenWorldSize.x * 0.8f;
            float height = screenWorldSize.y - screenWorldSize.x * 0.2f;
            return new Vector2(width, height);
        }

        void ResetImageEffect()
        {
            if (gameObject.activeInHierarchy)
            {
                foreach (var selected in WorkspaceUtils.SelectedLampItems)
                    ResetImageEffectOnLamp(selected);
            }
        }

        void ResetImageEffectOnLamp(LampItemView item)
        {
            var mapping = GetLampMapping(item);
            var lamp = item.lamp;

            if (_image != null)
            {
                lamp.SetEffect(_image);
                lamp.buffer.Setup(1);
            }

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
    }
}
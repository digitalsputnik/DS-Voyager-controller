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
            SelectionMove.onSelectionMoveEnded += SendImageToLamps;
            StartCoroutine(ResendImage());
        }

        void OnDestroy()
        {
            SelectionMove.onSelectionMoveEnded -= SendImageToLamps;
            StopCoroutine(ResendImage());
        }

        IEnumerator ResendImage()
        {
            while(true)
            {
                yield return new WaitForSeconds(0.5f);
                SendImageToLamps();
            }
        }

        public void SetImage(Image image)
        {
            _image = image;
            var texture = _image.image;
            _renderMesh.material.SetTexture("_MainTex", texture);
            ResizeMesh(texture);
            SendImageToLamps();
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

        void SendImageToLamps()
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

            if (_image != null)
            {
                lamp.SetEffect(_image);
                lamp.buffer.Setup(1);
            }

            lamp.SetMapping(mapping);
            RenderFrame(lamp);
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

        void RenderFrame(Lamps.Lamp lamp)
        {
            var texture = _image.image;
            var coords = VectorUtils.MapLampToVideoCoords(lamp, texture);
            var colors = TextureUtils.CoordsToColors(coords, texture);
            lamp.PushFrame(colors, 0);
        }
    }
}
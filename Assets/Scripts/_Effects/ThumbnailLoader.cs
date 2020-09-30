using System.Collections;
using DigitalSputnik;
using UnityEngine;
using UnityEngine.Video;

namespace VoyagerController.Effects
{
    public class ThumbnailLoader : MonoBehaviour
    {
        private const int THUMBNAIL_WIDTH = 160;
        private const int THUMBNAIL_HEIGHT = 90;
        private const float THUMBNAIL_TIMEOUT = 10.0f;
        private const float VIDEO_THUMBNAIL_TIME = 2.0f;

        public void LoadThumbnail(Effect effect, EffectHandler loaded)
        {
            switch (effect)
            {
                case VideoEffect video:
                    StartCoroutine(LoadVideoThumbnail(video, loaded));
                    break;
            }
        }

        private IEnumerator LoadVideoThumbnail(VideoEffect effect, EffectHandler loaded)
        {
            var render = CreateThumbnailRenderer();
            var player = SetupVideoPlayer(render, effect.Video.Path);
            var startTime = TimeUtils.Epoch;
            
            yield return new WaitUntil(() => player.isPrepared || Timeout(startTime));

            for (ushort t = 0; t < player.audioTrackCount; t++)
                player.SetDirectAudioMute(t, true);
            
            player.Play();

            yield return new WaitForSeconds(VIDEO_THUMBNAIL_TIME);

            var thumbnail = render.ToTexture2D();
            
            effect.Meta.Thumbnail = thumbnail;
            loaded?.Invoke(effect);
            
            Destroy(player);
            Destroy(render);
        }

        private VideoPlayer SetupVideoPlayer(RenderTexture render, string source)
        {
            var player = gameObject.AddComponent<VideoPlayer>();
            player.url = source;
            player.renderMode = VideoRenderMode.RenderTexture;
            player.targetTexture = render;
            player.Prepare();
            return player;
        }
        
        private static RenderTexture CreateThumbnailRenderer()
        {
            var render = new RenderTexture(
                THUMBNAIL_WIDTH, 
                THUMBNAIL_HEIGHT, 
                0, 
                RenderTextureFormat.ARGB32);
            render.Create();
            return render;
        }

        private static bool Timeout(double start) => TimeUtils.Epoch - start > THUMBNAIL_TIMEOUT;
    }
}
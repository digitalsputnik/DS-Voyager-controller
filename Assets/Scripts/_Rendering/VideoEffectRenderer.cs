using System;
using System.Collections;
using DigitalSputnik.Videos;
using UnityEngine;
using UnityEngine.Video;

namespace VoyagerController.Rendering
{
    [RequireComponent(typeof(VideoPlayer))]
    public class VideoEffectRenderer : MonoBehaviour
    {
        private static VideoEffectRenderer _instance;
        private void Awake() => _instance = this;

        public static VideoPlayer VideoPlayer => _instance._videoPlayer;
        public static RenderTexture RenderTexture => _instance._renderTexture;
        
        private VideoRenderState _state = new CheckIfRenderedState();
        private VideoPlayer _videoPlayer = null;
        private RenderTexture _renderTexture = null;

        private void Start()
        {
            _videoPlayer = GetComponent<VideoPlayer>();
        }

        private void Update()
        {
            _state = _state.Update();
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
    }
}

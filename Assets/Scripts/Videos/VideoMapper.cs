using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;
using VoyagerApp.Effects;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;
using VoyagerApp.Workspace.Views;

namespace VoyagerApp.UI
{
    public class VideoMapper : MonoBehaviour
    {
        public Transform MeshTransform => renderMesh.transform;

        Video video;
        VideoPlayer player;

        RenderTexture render;
        MeshRenderer renderMesh;

        bool stopRequested;

        void Start()
        {
            player = GetComponent<VideoPlayer>();
            renderMesh = GetComponent<MeshRenderer>();

            PlaymodeChanged(ApplicationState.Playmode.value);

            SelectionMove.onSelectionMoveEnded += SelectionMoved;
            ApplicationState.Playmode.onChanged += PlaymodeChanged;

            StartCoroutine(FpsCorrector());
        }

        void Update()
        {
            if (stopRequested)
            {
                player.Pause();
                stopRequested = false;
            }
        }

        private void OnDestroy()
        {
            SelectionMove.onSelectionMoveEnded -= SelectionMoved;
            ApplicationState.Playmode.onChanged -= PlaymodeChanged;
        }

        IEnumerator FpsCorrector()
        {
            CorrectFps();
            yield return new WaitForSeconds(Random.Range(0.0f, 60.0f));
            StartCoroutine(FpsCorrector());
        }

        public void CorrectFps()
        {
            if (video == null) return;

            long lampFrame = TimeUtils.GetFrameOfVideo(video);
            if (lampFrame != player.frame)
                SetFrame(lampFrame);
        }

        public void UpdateEffectSettings()
        {
            renderMesh.sharedMaterial.SetFloat("_Lift", (video.lift * 2.0f) - 1.0f);
            renderMesh.sharedMaterial.SetFloat("_Contrast", (video.contrast * 2.0f) - 1.0f);
            renderMesh.sharedMaterial.SetFloat("_Saturation", video.saturation * 2.0f);
        }

        #region Setting video
        public void SetVideo(Video video)
        {
            if (render != null)
                ClearUp();

            this.video = video;

            if (video != null)
            {
                SetupRenderTexture();
                PrepereVideoPlayer();
                SetupMeshSize();
            }
        }

        public void SetFrame(long frame)
        {
            player.frame = frame;
        }

        public void SetFps(float fps)
        {
            player.playbackSpeed = 1.0f / player.frameRate * fps;
            SetFrame(TimeUtils.GetFrameOfVideo(video, 0.3f));
        }

        void ClearUp()
        {
            player.Stop();
            player.targetTexture = null;

            Destroy(render);

            video = null;
        }

        void SetupRenderTexture()
        {
            render = new RenderTexture((int)video.width, (int)video.height, 32);
            render.Create();

            renderMesh.material.SetTexture("_MainTex", render);
            player.targetTexture = render;
        }

        void PrepereVideoPlayer()
        {
            player.url = video.path;
            player.waitForFirstFrame = true;
            player.started += PlayerStarted;
            player.Play();
        }

        void PlayerStarted(VideoPlayer _)
        {
            player.started -= PlayerStarted;
            player.skipOnDrop = true;
            CorrectFps();
            SetFps(video.fps);

            if (ApplicationState.Playmode.value == GlobalPlaymode.Pause)
            {
                SetFrame(TimeUtils.GetFrameOfVideo(video, -(TimeUtils.Epoch - ApplicationState.PlaymodePausedSince.value)));
                player.seekCompleted += SeekedForPause;
            }
            else
                PlaymodeChanged(ApplicationState.Playmode.value);
        }

        void SetupMeshSize()
        {
            Vector2 maxScale = CalculateMeshMaxScale();

            float maxScaleAspect = maxScale.x / maxScale.y;
            float videoAspect = (float)video.width / video.height;

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
        #endregion

        #region Handling position changed event
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

            if (video != null)
            {
                lamp.SetEffect(video);
                lamp.buffer.Setup(video.frames);
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
        #endregion

        #region Play / Pause / Stop
        void PlaymodeChanged(GlobalPlaymode value)
        {
            switch (value)
            {
                case GlobalPlaymode.Play:
                    StartCoroutine(SetCorrectFrameAndPlay());
                    break;
                case GlobalPlaymode.Pause:
                    if (video != null)
                        player.Pause();
                    break;
                case GlobalPlaymode.Stop:
                    if (video != null)
                    {
                        player.frame = 0;
                        stopRequested = true;
                    }
                    break;
            }
        }

        IEnumerator SetCorrectFrameAndPlay()
        {
            yield return new WaitForEndOfFrame();
            SetFrame(TimeUtils.GetFrameOfVideo(video));
            if (video != null)
                player.Play();
        }

        void SeekedForPause(VideoPlayer source)
        {
            player.Pause();
            player.seekCompleted -= SeekedForPause;
        }
        #endregion
    }
}

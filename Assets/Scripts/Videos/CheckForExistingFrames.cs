using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Projects;
using VoyagerApp.Utilities;

namespace VoyagerApp.UI
{
    public class CheckForExistingFrames : MonoBehaviour
    {
        public bool allRendered;
        [SerializeField] float requestTime = 1.0f;
        [SerializeField] Text progressText = null;
        [SerializeField] Image fillImage = null;

        ProjectLoadBuffer loading;
        bool done;

        public void Start()
        {
            InvokeRepeating("CheckForFrames", 0.5f, requestTime);
        }

        void CheckForFrames()
        {
            if (AllBuffered)
            {
                if (loading == null && !done)
                {
                    loading = new ProjectLoadBuffer(WorkspaceUtils.Lamps, UpdateFill);
                    loading.StartSending(false);
                }
            }
            else
            {
                UpdateFill((float)AllFramesBuffered / AllFrames * 0.9f);
                if (done) done = false;
                if (allRendered) allRendered = false;
            }
        }

        void UpdateFill(float value)
        {
            if (progressText == null) return;
            if (System.Math.Abs(value) < 0.01) return;


            progressText.text = $"UPLOADING PROGRESS {(int)(value * 100)}%";
            fillImage.fillAmount = value;

            if (value >= 1.0)
            {
                done = true;
                loading = null;
                allRendered = true;
            }
        }

        bool AllBuffered => WorkspaceUtils.Lamps.All(l => l.buffer.ExistingFramesCount == l.buffer.frames);
        long AllFrames => WorkspaceUtils.Lamps.Sum(l => l.buffer.frames);
        long AllFramesBuffered => WorkspaceUtils.Lamps.Sum(l => l.buffer.ExistingFramesCount);
    }
}
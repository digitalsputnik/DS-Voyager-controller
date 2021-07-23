using System;
using System.Linq;
using DigitalSputnik;
using DigitalSputnik.Voyager;
using UnityEngine;
using UnityEngine.UI;
using VoyagerController.Effects;
using VoyagerController.Workspace;

namespace VoyagerController.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class RenderLoadingBar : MonoBehaviour
    {
        [SerializeField] private Text _infoText = null;
        [SerializeField] private Text _processText = null;
        [SerializeField] private Image _fillImage = null;
        [SerializeField] private float _fillSpeed = 5.0f;

        private CanvasGroup _canvasGroup;
        
        private void Start()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Update()
        {
            var rendering = WorkspaceManager.GetItems<VoyagerItem>()
                .Select(i => i.LampHandle)
                .Where(l =>
                {
                    var meta = Metadata.Get<LampData>(l.Serial);
                    if (!(meta.Effect is VideoEffect)) return false;
                    return !meta.Rendered || meta.TotalMissingFrames > 0;
                }).ToArray();
            
            _canvasGroup.alpha = rendering.Any() ? 1.0f : 0.0f;

            if (!rendering.Any()) return;

            var metas = rendering.Select(l => Metadata.Get<LampData>(l.Serial)).ToArray();

            var renderedSum = metas.Sum(m => m.FrameBuffer.Count(f => f != null));
            var sum = metas.Sum(m => m.ConfirmedFrames.Length);
            var done = metas.Sum(m =>
            {
                var index = Array.IndexOf(metas, m);
                if (rendering[index].Connected)
                    return m.ConfirmedFrames.Length - m.TotalMissingFrames;
                return m.FrameBuffer.Count(f => f != null);
            }) + renderedSum;
            var process = (float) done / (sum * 2);

            _fillImage.fillAmount = process > _fillImage.fillAmount ? Mathf.Lerp(_fillImage.fillAmount, process, _fillSpeed * Time.deltaTime) : process;
            _processText.text = $"{(int) (process * 100)}%";
            _infoText.text = renderedSum != sum ? "RENDERING" : "UPLOADING MISSING FRAMES";
            _canvasGroup.alpha = renderedSum == sum ? 0.0f : 1.0f;
        }
    }
}
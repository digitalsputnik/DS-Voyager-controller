using System.Linq;
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

        private CanvasGroup _canvasGroup;
        
        private void Start()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Update()
        {
            var rendering = WorkspaceManager.GetItems<VoyagerItem>()
                .Select(i => Metadata.Get(i.LampHandle.Serial))
                .Where(meta =>
                {
                    if (!(meta.Effect is VideoEffect))
                        return false;
                    return !meta.Rendered;
                }).ToArray();
            
            _canvasGroup.alpha = rendering.Any() ? 1.0f : 0.0f;

            if (!rendering.Any()) return;

            var sum = rendering.Sum(m => m.ConfirmedFrames.Length);
            var done = rendering.Sum(m => m.ConfirmedFrames.Length -  m.TotalMissingFrames);
            var process = (float) done / sum;
            
            _fillImage.fillAmount = process;
            _processText.text = $"{(int) (process * 100)}%";
            _infoText.text = "Rendering";
        }
    }
}
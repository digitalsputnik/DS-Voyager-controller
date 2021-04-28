using System;
using System.Collections;
using DigitalSputnik.Videos;
using UnityEngine;
using UnityEngine.UI;
using VoyagerController.Effects;

namespace VoyagerController.UI
{
    public class SetEffectItem : MonoBehaviour
    {
        [SerializeField] private RawImage _thumbnailImage = null;
        [SerializeField] private Button _removeButton = null;
        [SerializeField] private Text _nameText = null;
        [SerializeField] private Text _infoText = null;

        private string _overlay;
        private Effect _effect;
        private Action _action;

        public void Setup(Effect effect, Action action)
        {
            _effect = effect;
            _action = action;
            UpdateInterface();
        }

        public void Remove() => EffectManager.RemoveEffect(_effect);

        public void Select() => _action?.Invoke();

        public void SetOverlay(string overlay)
        {
            _overlay = overlay;
            UpdateInterface();
        }
        
        private void UpdateInterface()
        {
            _thumbnailImage.enabled = false;
            _nameText.text = _effect.Name;

            if (string.IsNullOrEmpty(_overlay))
            {
                switch (_effect)
                {
                    case VideoEffect video:
                        _infoText.text =
                            "duration \n" +
                            VideoTimeCode(video.Video) + "\n" +
                            "resolution \n" +
                            video.Video.Width + "x" + video.Video.Height;
                        break;
                    case ImageEffect image:
                        _infoText.text =
                            "resolution \n" +
                            image.ImageTexture.width + "x" + image.ImageTexture.height;
                        break;
                }   
            }
            else
            {
                _infoText.text = _overlay;
            }

            if (EffectManager.IsEffectPreset(_effect))
                _removeButton.gameObject.SetActive(false);

            StartCoroutine(WaitForThumbnail());
        }

        private IEnumerator WaitForThumbnail()
        {
            yield return new WaitUntil(() => _effect.Meta.Thumbnail != null);
            _thumbnailImage.texture = _effect.Meta.Thumbnail;
            _thumbnailImage.enabled = true;
        }

        private static string VideoTimeCode(Video video)
        {
            if (video == null) return "error";
            
            var time = TimeSpan.FromSeconds(video.Duration);
            var frames = (int)Mathf.Round((float) ((float)time.Milliseconds / 1000 * video.Fps));
            return time.ToString(@"hh\:mm\:ss\:") + frames;
        }
    }
}
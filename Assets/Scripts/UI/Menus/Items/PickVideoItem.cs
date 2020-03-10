using System;
using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Effects;
using VoyagerApp.UI.Overlays;

namespace VoyagerApp.UI.Menus
{
	public class PickVideoItem : MonoBehaviour
	{
        [SerializeField] RawImage thumbnailImage    = null;
        [SerializeField] Text nameText              = null;
        [SerializeField] Text infoText              = null;

        public Video video { get; set; }

        public void SetVideo(Video video)
        {
            this.video = video;

            thumbnailImage.texture = video.thumbnail;
            nameText.text = video.name;
            infoText.text = InfoText;
        }

        string InfoText
        {
            get
            {
                return $"FPS: {video.fps}\n" +
                       $"SIZE: {video.width} x {video.height}\n" +
                       $"DURATION: {video.duraction} SEC";
            }
        }

        public void OnClick()
        {
            GetComponentInParent<PickVideoMenu>().PickVideo(video);
        }

        public void Remove()
        {
            DialogBox.Show(
                "ARE YOU SURE?",
                "Are you sure you want to remove this video from project?",
                new string[] { "CANCEL", "OK" },
                new Action[] { null, () => { EffectManager.RemoveEffect(video); } }
                );
        }
	}
}
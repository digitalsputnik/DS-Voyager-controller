using System;
using UnityEngine;

namespace VoyagerController
{
    [Serializable]
    public class PictureData : Data
    {
        public Texture2D Texture { get; set; }
    }
}
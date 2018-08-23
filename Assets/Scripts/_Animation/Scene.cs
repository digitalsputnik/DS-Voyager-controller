using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Voyager.Networking;

namespace Voyager.Animation
{
	[Serializable]
	public class Scene
	{
		public double TimeStamp;
		public List<Layer> Layers;
		public bool ArtNetMode;
		public bool sACNMode;
        
        public void AddLayer(Layer layer)
        {
            if (Layers == null)
                Layers = new List<Layer>();

            Layers.Add(layer);
        }

        public void AddLatestTimeStamp()
        {
			TimeStamp = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds + NetworkManager.GetTimesyncOffset();
        }
	}   
}
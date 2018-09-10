using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Voyager.Animation
{
	[Serializable]
	public class Layer
	{
		public string LayerID;
        public List<Stroke> Strokes;
        public bool LayerActive;
        [NonSerialized]
        public Scene scene;

        [NonSerialized]
        public Dictionary<Pixel, List<Stroke>> PixelToStrokeIDDictionary;

        public Layer(string ID, Scene ParentScene, bool Active = true)
        {
            LayerID = ID;
            scene = ParentScene;
            if (ParentScene != null)
                scene.AddLayer(this);

            LayerActive = Active;
            PixelToStrokeIDDictionary = new Dictionary<Pixel, List<Stroke>>();
        }

        public void AddStroke(Stroke stroke)
        {
            if (Strokes == null)
                Strokes = new List<Stroke>();

            //Add stroke to list
            Strokes.Add(stroke);
            stroke.layer = this;
            //Order list
            Strokes = Strokes.OrderByDescending(s => s.CreationTimestamp).ToList();
        }

        /// <summary>
        /// Removes stroke from layer
        /// </summary>
        /// <param name="stroke"></param>
        public void RemoveStroke(Stroke stroke)
        {
            if (!Strokes.Contains(stroke))
                return;

            foreach (var pixel in stroke.ControlledPixels)
            {
                PixelToStrokeIDDictionary[pixel].Remove(stroke);
            }
            //TODO: Remove!?
            stroke.ControlledPixels.Clear();

            Strokes.Remove(stroke);
        }

        public Stroke GetStrokeByID(string strokeID)
        {
            var latestStroke = Strokes.Last();
            foreach (var stroke in Strokes)
            {
                if (stroke.StrokeID == strokeID)
                {
                    return stroke;
                }
                if (stroke.ControlledPixels != null)
                {
                    if (stroke.ControlledPixels.Count > 0)
                    {
                        latestStroke = stroke;
                    }
                }
            }

            return latestStroke;
        }

        /// <summary>
        /// Implements "Select Stroke functionality"
        /// </summary>
        /// <param name="pixel"></param>
        /// <returns></returns>
        public Stroke SelectStrokeFromPixel(Pixel pixel)
        {
            if (PixelToStrokeIDDictionary.ContainsKey(pixel))
            {
                return PixelToStrokeIDDictionary[pixel].FirstOrDefault();
            }
            else
            {
                //TODO: Better default return
                return null;
            }
        }

        /// <summary>
        /// Removes invisible strokes from layer
        /// </summary>
        public void RemoveInvisibleStrokes(Stroke IgnoreStroke = null)
        {
			var reverseStrokes = Strokes.OrderBy(s => s.CreationTimestamp).ToList();
            List<string> VisibleStrokeIDs = PixelToStrokeIDDictionary.Select(x => x.Value.FirstOrDefault().StrokeID).ToList();
            foreach (var stroke in reverseStrokes)
            {
                if (!VisibleStrokeIDs.Contains(stroke.StrokeID))
                {
                    if (IgnoreStroke == null)
                    {
                        RemoveStroke(stroke);
                    }
                    else
                    {
                        if (IgnoreStroke.StrokeID != stroke.StrokeID)
                        {
                            RemoveStroke(stroke);
                        }
                    }
                }
            }
        }
	}
}


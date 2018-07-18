using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Voyager.Lamps
{
	[Serializable]
    public class Stroke
    {
        public string StrokeID;
        public double CreationTimestamp;
        public double TimeStamp;
        public double StartTime;
        public List<int[]> Colors;
        public string Animation;
        public Dictionary<string, int[]> Properties;
        public float Duration;
        public int Universe;
        public int DMXOffset;

        //Pixel information for lamp
        public int TotalPixelCount;
        public Dictionary<string, SortedDictionary<int, int>> PixelQueueToControlledPixel;
        //public Dictionary<string, SortedDictionary<float, int>> PixelTimestampToControlledPixel;

        [NonSerialized]
        public Layer layer;
        //Pixel information for UI
        [NonSerialized]
        public List<Pixel> ControlledPixels;
        //[NonSerialized]
        //public SortedDictionary<int, Pixel> TimestampToPixel;

        public Stroke(string ID, Layer ParentLayer, Dictionary<string, SortedDictionary<int, int>> LampPixQueueToPixel = null, int pixelCount = 0, string animation = "", Dictionary<string, int[]> properties = null)
        {
            //Timestamps
            CreationTimestamp = GetCurrentTimestampUTC();
            Duration = 2000f;
            StartTime = GetStartTimeFromProperties(properties);

            //Add layer reference!
            layer = ParentLayer;
            ControlledPixels = new List<Pixel>();
            ChangeStroke(ID, LampPixQueueToPixel, pixelCount, animation, properties);
            if (layer != null)
                layer.AddStroke(this);
        }

        public void ChangeStroke(string ID = "", Dictionary<string, SortedDictionary<int, int>> LampPixQueueToPixel = null, int pixelCount = 0, string animation = "", Dictionary<string, int[]> properties = null)
        {
            StrokeID = ID;
            TimeStamp = GetCurrentTimestampUTC();
            StartTime = GetStartTimeFromProperties(properties);

            if (LampPixQueueToPixel == null)
            {
                LampPixQueueToPixel = new Dictionary<string, SortedDictionary<int, int>>();
            }
            PixelQueueToControlledPixel = LampPixQueueToPixel;
            TotalPixelCount = pixelCount;
            Animation = animation;
            Properties = properties;
        }

        public void AddLatestTimestamp()
        {
            TimeStamp = GetCurrentTimestampUTC();
        }

        private double GetCurrentTimestampUTC()
        {
            return (double)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        private double GetStartTimeFromProperties(Dictionary<string, int[]> properties = null)
        {
            var response = (double)(DateTime.Today - new DateTime(1970, 1, 1)).TotalSeconds;

            if (properties != null)
            {
                if (properties.ContainsKey("StartTime"))
                {
                    var startTime = properties["StartTime"];
                    var today = DateTime.Today;
                    var time = new TimeSpan(0, startTime[0], startTime[1], startTime[2], startTime[3]);
                    //TODO: Take time offset into account!
                    response = (double)((today + time) - new DateTime(1970, 1, 1)).TotalSeconds;
                }
            }
            return response;
        }

        /// <summary>
        /// Adds pixel to stroke and layer
        /// </summary>
        /// <param name="pixel"></param>
        public void AddPixel(Pixel pixel, bool isStrokeActive = true)
        {
            //If pixel has been added, the order is not changed!
            if (ControlledPixels.Contains(pixel))
                RemovePixel(pixel);
            //return;

            ControlledPixels.Add(pixel);

            //Add pixel to queues
            string LampMac = pixel.transform.parent.GetComponent<Ribbon>().Mac;
            if (PixelQueueToControlledPixel.ContainsKey(LampMac))
            {
                PixelQueueToControlledPixel[LampMac].Add(TotalPixelCount, pixel.ID);
            }
            else
            {
                var QueueToControlledPixel = new SortedDictionary<int, int>();
                QueueToControlledPixel.Add(TotalPixelCount, pixel.ID);
                PixelQueueToControlledPixel.Add(LampMac, QueueToControlledPixel);
            }
            TotalPixelCount++;

            //Add stroke to pixels
            if (layer.PixelToStrokeIDDictionary.ContainsKey(pixel))
            {
                var pixelStrokes = layer.PixelToStrokeIDDictionary[pixel];
                //var strokesOnTop = pixelStrokes.Any(s => s.CreationTimestamp > ActiveStroke.CreationTimestamp);
                //Add stroke to pixel and order
                pixelStrokes.Add(this);
                pixelStrokes = pixelStrokes.OrderByDescending(s => s.CreationTimestamp).ToList();
                layer.PixelToStrokeIDDictionary[pixel] = pixelStrokes;
            }
            else
            {
                List<Stroke> strokes = new List<Stroke> { this };
                layer.PixelToStrokeIDDictionary.Add(pixel, strokes);
            }

            if (isStrokeActive)
            {
                PixelSelectionOn(pixel);
            }

            layer.RemoveInvisibleStrokes();
            TimeStamp = GetCurrentTimestampUTC();
        }

        /// <summary>
        /// Removes pixel from Stroke and all associated variables
        /// </summary>
        /// <param name="pixel"></param>
        public void RemovePixel(Pixel pixel)
        {
            //TimeStamp = GetCurrentTimestampUTC();

            //Remove pixel from controlled pixels
            if (!ControlledPixels.Contains(pixel))
            {
                ControlledPixels.Remove(pixel);
            }

            //Correct dictionaries according to pixel order (queue number reduction for each pixel which is higher)
            string LampMac = pixel.transform.parent.GetComponent<Ribbon>().Mac;
            if (PixelQueueToControlledPixel.ContainsKey(LampMac))
            {
                //Removes current
                var QueueNumber = PixelQueueToControlledPixel[LampMac].FirstOrDefault(d => d.Value == pixel.ID).Key;
                PixelQueueToControlledPixel[LampMac].Remove(QueueNumber);
                //Each queuenumber which is larger than deleted is decrease by one
                for (int q = QueueNumber + 1; q < TotalPixelCount; q++)
                {
                    var pixelMac = PixelQueueToControlledPixel.FirstOrDefault(x => x.Value.ContainsKey(q)).Key;
                    var pixelID = PixelQueueToControlledPixel[pixelMac][q];
                    PixelQueueToControlledPixel[pixelMac].Remove(q);
                    PixelQueueToControlledPixel[pixelMac].Add(q - 1, pixelID);
                }
            }
            TotalPixelCount--;

            //TODO: Remove pixel from strokes!
            if (layer.PixelToStrokeIDDictionary.ContainsKey(pixel))
            {
                var pixelStrokes = layer.PixelToStrokeIDDictionary[pixel];
                //var strokesOnTop = pixelStrokes.Any(s => s.CreationTimestamp > ActiveStroke.CreationTimestamp);
                //Add stroke to pixel and order
                pixelStrokes.Remove(this);
                pixelStrokes = pixelStrokes.OrderByDescending(s => s.CreationTimestamp).ToList();
                layer.PixelToStrokeIDDictionary[pixel] = pixelStrokes;
            }

            //Turn pixel selection off
            PixelSelectionOff(pixel);
        }

        /// <summary>
        /// Turns on pixel selection bar
        /// </summary>
        /// <param name="pixel"></param>
        public void PixelSelectionOn(Pixel pixel)
        {
            if (layer.PixelToStrokeIDDictionary[pixel].FirstOrDefault().StrokeID == StrokeID)
            {
                //Stroke is visible!
                pixel.updateSelectionPixel(1);
            }
            else
            {
                //Stroke is not visible/under another stroke
                pixel.updateSelectionPixel(2);
            }
        }

        /// <summary>
        /// Turns off pixel selection bar
        /// </summary>
        /// <param name="pixel"></param>
        public void PixelSelectionOff(Pixel pixel)
        {
            pixel.updateSelectionPixel(0);
        }

        /// <summary>
        /// Turns on selection for entire stroke
        /// </summary>
        public void SelectionOn()
        {
            foreach (var pixel in ControlledPixels)
            {
                PixelSelectionOn(pixel);
            }
        }

        /// <summary>
        /// Turns off selection for entire stroke
        /// </summary>
        public void SelectionOff()
        {
            foreach (var pixel in ControlledPixels)
            {
                pixel.updateSelectionPixel(0);
            }
        }
    }

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

    [Serializable]
    public class Scene
    {
        public double TimeStamp { get; set; }
        public List<Layer> Layers { get; set; }
        public bool ArtNetMode { get; set; }
        public bool sACNMode { get; set; }
        public void AddLayer(Layer layer)
        {
            if (Layers == null)
                Layers = new List<Layer>();

            Layers.Add(layer);
        }

        public void AddLatestTimeStamp()
        {
            TimeStamp = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        }
    }
}
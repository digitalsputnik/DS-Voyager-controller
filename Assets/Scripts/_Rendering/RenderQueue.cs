using System.Collections.Generic;
using System.Linq;
using DigitalSputnik.Voyager;
using VoyagerController.Effects;

namespace VoyagerController.Rendering
{
    internal class RenderQueue : Queue<KeyValuePair<VideoEffect, List<VoyagerLamp>>>
    {
        public static RenderQueue Create(IEnumerable<VoyagerLamp> lamps)
        {
            var dictionary = new Dictionary<VideoEffect, List<VoyagerLamp>>();
            var queue = new RenderQueue();

            foreach (var lamp in lamps)
            {
                var effect = (VideoEffect) ApplicationManager.Lamps.GetMetadata(lamp.Serial).Effect;
                if (!dictionary.ContainsKey(effect))
                    dictionary.Add(effect, new List<VoyagerLamp>());
                dictionary[effect].Add(lamp);
            }

            foreach (var pair in dictionary.OrderByDescending(d => d.Value.Count))
                queue.Enqueue(pair);

            return queue;
        }
    }
}
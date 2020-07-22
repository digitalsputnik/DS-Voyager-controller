using System;
using Newtonsoft.Json;

namespace VoyagerApp.Projects
{
    [Serializable]
    public class ProjectSaveData
    {
        public string version;
        [JsonProperty("app_version")]
        public string appVersion;
        public Effect[] effects;
        public Lamp[] lamps;
        public Item[] items;
        public float[] camera;
    }

    public class Effect
    {
        public string id;
        public string name;
        public string type;
        public float lift = 0.5f;
        public float contrast = 0.5f;
        public float saturation = 0.5f;
        public float blur = 0.0f;
    }

    [Serializable]
    public class Video : Effect
    {
        public long frames;
        public string file;
        public int fps;
    }

    [Serializable]
    public class VideoPreset : Effect { }

    [Serializable]
    public class Image : Effect
    {
        public byte[] data;
    }

    [Serializable]
    public class Spout : Effect
    {
        public string source;
    }
     
    [Serializable]
    public class Syphon : Effect
    {
        public string server;
        public string application;
    }

    [Serializable]
    public class Lamp
    {
        public string serial;
        public int length;
        public string effect;
        public string address;
        public float[] itsh;
        public float[] mapping;
        public byte[][] buffer;
    }

    [Serializable]
    public class Item
    {
        public string type;
        public float[] position;
        public float scale;
        public float rotation;
    }

    [Serializable]
    public class LampItem : Item
    {
        public string serial;
    }

    [Serializable]
    public class PictureItem : Item
    {
        public int width;
        public int height;
        public byte[] data;
        public int order;
    }

    [Serializable]
    public class WorkspaceSaveData
    {
        public Item[] items;
        public float[] camera;
    }
}
using System;

namespace VoyagerApp.Projects
{
    [Serializable]
    public class ProjectSaveData
    {
        public string version;
        public Video[] videos;
        public Lamp[] lamps;
        public Item[] items;
        public float[] camera;
    }

    [Serializable]
    public class Video
    {
        public string guid;
        public long frames;
        public int fps;
        public string url;
    }

    [Serializable]
    public class Lamp
    {
        public string serial;
        public int length;
        public string video;
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
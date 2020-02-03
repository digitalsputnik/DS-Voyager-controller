﻿using UnityEngine;

namespace VoyagerApp.Effects
{
    public abstract class Effect
    {
        public bool preset;
        public string name;
        public string id;
        public Texture2D thumbnail;
        public EventValue<bool> available = new EventValue<bool>(false);
    }

    public class Video : Effect
    {
        public string file;
        public string path;
        public long frames;
        public int fps;
        public uint width;
        public uint height;
        public double startTime;

        public double duraction => (double)frames / fps;
    }

    public class Stream : Effect
    {

    }

    public class SyphonStream : Stream
    {
        public string server;
        public string application;
    }

    public class SpoutStream : Stream
    {
        public string source;
    }
}
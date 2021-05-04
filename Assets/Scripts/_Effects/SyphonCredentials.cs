using System;

namespace VoyagerController.Effects
{
    [Serializable]
    public struct SyphonCredentials
    {
        public string Server { get; set; }
        public string Application { get; set; }

        public SyphonCredentials(string server, string application)
        {
            Server = server;
            Application = application;
        }
    }
}
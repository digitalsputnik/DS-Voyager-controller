using DigitalSputnik;
using UnityEngine;

namespace VoyagerController
{
    public class UnityDebugConsole : IDebugConsole
    {
        public void LogInfo(object info)
        {
            Debug.Log(info);
        }
        
        public void LogWarning(object warning)
        {
            Debug.LogWarning(warning);
        }

        public void LogError(object error)
        {
            Debug.LogError(error);
        }
    }
}
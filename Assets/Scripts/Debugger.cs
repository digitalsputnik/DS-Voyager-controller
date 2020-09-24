using UnityEngine;

namespace VoyagerController
{
    public static class Debugger
    {
        public static void LogInfo(object info)
        {
            Debug.Log(info);
        }
        
        public static void LogWarning(object warning)
        {
            Debug.LogWarning(warning);
        }

        public static void LogError(object error)
        {
            Debug.LogError(error);
        }
    }
}
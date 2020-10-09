// KlakSyphon - Syphon plugin for Unity
// https://github.com/keijiro/KlakSyphon

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Klak.Syphon
{
    internal static class SyphonCommon
    {
        // Apply the current color space setting.
        // Actually this is needed only once, but we do every time for simplicity.
        internal static void ApplyCurrentColorSpace()
        {
            if (QualitySettings.activeColorSpace == ColorSpace.Linear)
                Plugin_EnableColorSpaceConversion();
            else
                Plugin_DisableColorSpaceConversion();
        }

        [DllImport("KlakSyphon")]
        static extern void Plugin_EnableColorSpaceConversion();

        [DllImport("KlakSyphon")]
        static extern void Plugin_DisableColorSpaceConversion();
    }

    public static class SyphonHelper
    {
        public static (string, string)[] GetListOfServers()
        {
            var names = new List<(string, string)>();
            var list = Plugin_CreateServerList();
            var count = Plugin_GetServerListCount(list);

            if (count == 0)
                return null;

            for (int i = 0; i < count; i++)
            {
                var pName = Plugin_GetNameFromServerList(list, i);
                var pAppName = Plugin_GetAppNameFromServerList(list, i);

                var name = (pName != IntPtr.Zero) ? Marshal.PtrToStringAnsi(pName) : "";
                var appName = (pAppName != IntPtr.Zero) ? Marshal.PtrToStringAnsi(pAppName) : "";

                names.Add((name, appName));
            }

            return names.ToArray();
        }

        #region Native plugin entry points

        [DllImport("KlakSyphon")]
        static extern IntPtr Plugin_CreateServerList();

        [DllImport("KlakSyphon")]
        static extern void Plugin_DestroyServerList(IntPtr list);

        [DllImport("KlakSyphon")]
        static extern int Plugin_GetServerListCount(IntPtr list);

        [DllImport("KlakSyphon")]
        static extern IntPtr Plugin_GetNameFromServerList(IntPtr list, int index);

        [DllImport("KlakSyphon")]
        static extern IntPtr Plugin_GetAppNameFromServerList(IntPtr list, int index);

        #endregion
    }
}

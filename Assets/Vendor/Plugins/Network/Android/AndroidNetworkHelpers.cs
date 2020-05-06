#if UNITY_ANDROID

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public static class AndroidNetworkHelpers
{
    public static void RequestPermissions()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation))
            Permission.RequestUserPermission(Permission.CoarseLocation);
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            Permission.RequestUserPermission(Permission.FineLocation);
        if (!Permission.HasUserAuthorizedPermission("android.permission.CHANGE_WIFI_STATE"))
            Permission.RequestUserPermission("android.permission.CHANGE_WIFI_STATE");
        if (!Permission.HasUserAuthorizedPermission("android.permission.ACCESS_WIFI_STATE"))
            Permission.RequestUserPermission("android.permission.ACCESS_WIFI_STATE");
    }

    public static bool HasPermissions()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation)) return false;
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation)) return false;
        if (!Permission.HasUserAuthorizedPermission("android.permission.CHANGE_WIFI_STATE")) return false;
        if (!Permission.HasUserAuthorizedPermission("android.permission.ACCESS_WIFI_STATE")) return false;
        return true;
    }

    public static void ScanForSsids(MonoBehaviour behaviour, float time, Action<string[]> callback)
    {
        behaviour.StartCoroutine(IEnumScanForSsid(time, callback));
    }

    static IEnumerator IEnumScanForSsid(float time, Action<string[]> callback)
    {
        if (!HasPermissions()) RequestPermissions();

        yield return new WaitUntil(() => HasPermissions());

        List<string> ssids = new List<string>();

        var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        var context = activity.Call<AndroidJavaObject>("getApplicationContext");
        var wifiPlugin = new AndroidJavaObject("com.example.wifiplugin.WifiScanner");
        var proxy = new JavaWifiSsidCallback((ssid) =>
        {
            if (!ssids.Contains(ssid)) ssids.Add(ssid);
        });

        wifiPlugin.Call("startScanning", context, proxy);

        yield return new WaitForSeconds(time);

        callback.Invoke(ssids.ToArray());
    }

    class JavaWifiSsidCallback : AndroidJavaProxy
    {
        internal Action<string> _callback;

        public JavaWifiSsidCallback(Action<string> callback) : this()
        {
            _callback = callback;
        }

        public JavaWifiSsidCallback() : base("com.example.wifiplugin.PluginCallBack") { }

        public void onSuccess(string ssid)
        {
            _callback?.Invoke(ssid);
        }

        public void onError(string error)
        {
            Debug.Log($"SSIDList: Error : {error}");
        }
    }
}

#endif
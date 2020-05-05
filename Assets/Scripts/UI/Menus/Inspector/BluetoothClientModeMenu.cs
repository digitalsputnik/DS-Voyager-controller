using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

namespace VoyagerApp.UI.Menus
{
    public class BluetoothClientModeMenu : Menu
    {
        [SerializeField] GameObject _ssidListObj = null;
        [SerializeField] ListPicker _ssidList = null;
        [SerializeField] Button _ssidRefreshBtn = null;
        [SerializeField] GameObject _ssidFieldObj = null;
        [SerializeField] InputField _ssidField = null;
        [SerializeField] Button _setSsidScan = null;
        [SerializeField] Button _setBtn = null;
        [SerializeField] string[] _loadingAnim = null;
        [SerializeField] float _animationSpeed = 0.6f;

        bool _loading;
        string[] _ids;

        internal override void OnShow()
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                string ssid = IOSNetworkHelpers.GetCurrentSsidName();
                _ssidField.text = (ssid == "unknown") ? ApplicationSettings.IOSBluetoothWifiSsid : ssid;
                _setSsidScan.interactable = false;
                ShowTypeSsid();
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                ShowScanSsids();
            }
        }

        internal override void OnHide()
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
                ApplicationSettings.IOSBluetoothWifiSsid = _ssidField.text;
        }

        public void ShowScanSsids()
        {
            _ssidListObj.gameObject.SetActive(true);
            _ssidFieldObj.gameObject.SetActive(false);
            StartLoadingSsids();
        }

        public void ShowTypeSsid()
        {
            if (!_loading)
            {
                _ssidListObj.gameObject.SetActive(false);
                _ssidFieldObj.gameObject.SetActive(true);
            }
        }

        public void ReloadSsidList()
        {
            StartLoadingSsids();
        }

        public void SetBluetoothIds(string[] ids)
        {
            _ids = ids;
        }

        void StartLoadingSsids()
        {
            _ssidList.index = 0;
            _setBtn.interactable = false;
            _ssidList.interactable = false;
            _ssidRefreshBtn.interactable = false;

            AndroidNetworkHelpers.ScanForSsids(this, 5.0f, OnSsidListReceived);

            _loading = true;

            StartCoroutine(IEnumLoadingAnimation());
        }

        IEnumerator IEnumLoadingAnimation()
        {
            int i = 0;
            while (_loading)
            {
                _ssidList.SetItems(_loadingAnim[i]);
                yield return new WaitForSeconds(_animationSpeed);
                if (++i >= _loadingAnim.Length)
                    i = 0;
            }
        }

        void OnSsidListReceived(string[] ssids)
        {
            _loading = false;

            if (ssids.Length > 0)
            {
                _setBtn.interactable = true;
                _ssidList.interactable = true;
                _ssidRefreshBtn.interactable = true;
                _ssidList.SetItems(ssids);
            }
            else
            {
                _ssidList.SetItems("Not found");
            }
        }
    }

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
            if (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation))              return false;
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))                return false;
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
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using VoyagerApp.Lamps;
using VoyagerApp.Lamps.Voyager;
using VoyagerApp.Networking.Voyager;
using VoyagerApp.UI.Overlays;
using VoyagerApp.Utilities;

namespace VoyagerApp.UI.Menus
{
    public class ClientModeMenu : Menu
    {
        [SerializeField] float ssidPollTimeout      = 10.0f;
        [SerializeField] GameObject ssidListObj     = null;
        [SerializeField] ListPicker ssidList        = null;
        [SerializeField] Button ssidRefreshBtn      = null;
        [SerializeField] GameObject ssidFieldObj    = null;
        [SerializeField] InputField ssidField       = null;
        [SerializeField] InputField passwordField   = null;
        [SerializeField] GameObject status          = null;
        [SerializeField] Button setBtn              = null;
        [SerializeField] string[] loadingAnim       = null;
        [SerializeField] float animationSpeed       = 0.6f;

        public List<string> foundSsidList = new List<string>();
        List<BLEItem> selectedLamps;

        Dictionary<Lamp, List<string>> lampToSsids = new Dictionary<Lamp, List<string>>();
        bool loading;
        bool settingBleLamps = false;

        public override void Start()
        {
            base.Start();
            TypeSsidBtnClick();
        }

        internal override void OnShow()
        {
            if (ssidFieldObj.activeSelf) TypeSsidBtnClick();
            status.SetActive(false);
            setBtn.onClick.RemoveAllListeners();
            setBtn.onClick.AddListener(Set);
        }

        internal override void OnHide()
        {
            lampToSsids.Clear();
            settingBleLamps = false;
            StartCoroutine(DisconnectBleLamps());
        }

        public void ScanForSsidsBtnClick()
        {
            ssidListObj.gameObject.SetActive(true);
            ssidFieldObj.gameObject.SetActive(false);
            StartLoading();
        }

        public void ReloadSsidList()
        {
            StartLoading();
        }

        public void TypeSsidBtnClick()
        {
            if (!loading)
            {
                ssidListObj.gameObject.SetActive(false);
                ssidFieldObj.gameObject.SetActive(true);
                if (WorkspaceUtils.SelectedVoyagerLamps.Count > 0 && string.IsNullOrEmpty(ssidField.text))
                    ssidField.text = WorkspaceUtils.SelectedVoyagerLamps[0].activePattern;
            }
        }

        #region Lamp SSIDS

        void StartLoading()
        {
            ssidList.index = 0;
            ssidList.interactable = false;
            ssidRefreshBtn.interactable = false;

            if(settingBleLamps != true)
            {
                lampToSsids.Clear();
                StartCoroutine(IEnumGetSsidListFromLamps());
            }
            else
            {
                foundSsidList.Clear();
                if (Application.platform == RuntimePlatform.Android)
                {
                    loading = true;
                    StartCoroutine(AndroidSsidTest());
                }
            }

            StartCoroutine(IEnumLoadingAnimation());
        }

        IEnumerator IEnumGetSsidListFromLamps()
        {
            loading = true;
            List<List<string>> allSsids = new List<List<string>>();
            int count = 0;
            int gathered = 0;

            foreach (var lamp in WorkspaceUtils.SelectedLamps)
            {
                if (lamp.connected)
                {
                    if (lampToSsids.ContainsKey(lamp))
                        allSsids.Add(lampToSsids[lamp]);
                    else
                        PollSsidsFromLamp(lamp);
                }
            }

            void PollSsidsFromLamp(Lamp lamp)
            {
                count++;
                NetUtils.VoyagerClient.GetSsidListFromLamp(
                    lamp,
                    OnSsidsReceived,
                    ssidPollTimeout);
            }

            void OnSsidsReceived(Lamp lamp, string[] ssids)
            {
                if (!lampToSsids.ContainsKey(lamp))
                {
                    lampToSsids.Add(lamp, ssids.ToList());
                    allSsids.Add(ssids.ToList());
                    gathered++;
                }
            }

            double starttime = TimeUtils.Epoch;
            double timeout = ssidPollTimeout;

            yield return new WaitWhile(() =>
                (gathered != count) && ((TimeUtils.Epoch - starttime) < timeout)
            );

            List<string> returnSsids = new List<string>();

            if (allSsids.Count != 0)
            {
                var intersection = allSsids
                .Skip(1)
                .Aggregate(
                    new HashSet<string>(allSsids.First()), (h, e) => {
                        h.IntersectWith(e);
                        return h;
                    });
                returnSsids = intersection.ToList();
                returnSsids = returnSsids
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct()
                    .ToList();
            }

            OnSsidListReceived(returnSsids);
            loading = false;
        }

        IEnumerator IEnumLoadingAnimation()
        {
            int i = 0;
            while (loading)
            {
                ssidList.SetItems(loadingAnim[i]);
                yield return new WaitForSeconds(animationSpeed);
                if (++i >= loadingAnim.Length) i = 0;
            }
        }

        void OnSsidListReceived(List<string> ssids)
        {
            if (ssids.Count > 0)
            {
                ssidList.SetItems(ssids.ToArray());
                ssidList.interactable = true;
                setBtn.interactable = true;
                ssidRefreshBtn.interactable = true;
            }
            else
            {
                ssidList.SetItems("Not found");
            }
        }

        #endregion

        #region AndroidBleLampsTest

        public void SetupBluetooth(List<BLEItem> _selectedLamps)
        {
            settingBleLamps = true;
            selectedLamps = null;
            selectedLamps = _selectedLamps;
            setBtn.onClick.RemoveAllListeners();
            setBtn.onClick.AddListener(SetBluetooth);
            StartCoroutine(ConnectLamps());
        }

        IEnumerator ConnectLamps()
        {
            foreach (var lamp in selectedLamps)
            {
                lamp.settingClient = true;
                lamp.Connect();
                yield return new WaitUntil(() => lamp.connected);
            }
        }

        IEnumerator DisconnectBleLamps()
        {
            foreach (var lamp in selectedLamps)
            {
                if (lamp.connected)
                {
                    lamp.settingClient = false;
                    lamp.Disconnect();
                    yield return new WaitUntil(() => lamp.connected == false);
                }
                else if (lamp.connecting)
                {
                    yield return new WaitUntil(() => lamp.connected == true);
                    lamp.settingClient = false;
                    lamp.Disconnect();
                    yield return new WaitUntil(() => lamp.connected == false);
                }
            }
        }

        bool AndroidPremissions()
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation))
                return false;
            else if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
                return false;
            else if (!Permission.HasUserAuthorizedPermission("android.permission.CHANGE_WIFI_STATE"))
                return false;
            else if (!Permission.HasUserAuthorizedPermission("android.permission.ACCESS_WIFI_STATE"))
                return false;
            else return true;
        }

        IEnumerator AndroidSsidTest()
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.CoarseLocation))
                Permission.RequestUserPermission(Permission.CoarseLocation);
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
                Permission.RequestUserPermission(Permission.FineLocation);
            if (!Permission.HasUserAuthorizedPermission("android.permission.CHANGE_WIFI_STATE"))
                Permission.RequestUserPermission("android.permission.CHANGE_WIFI_STATE");
            if (!Permission.HasUserAuthorizedPermission("android.permission.ACCESS_WIFI_STATE"))
                Permission.RequestUserPermission("android.permission.ACCESS_WIFI_STATE");

            yield return new WaitUntil(() => AndroidPremissions());

            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");
            AndroidJavaObject wifiPlugin = new AndroidJavaObject("com.example.wifiplugin.WifiScanner");

            var callback = new AndroidPluginCallBack();
            callback._callback = OnSsidScanned;

            object[] parameters = new object[2];
            parameters[0] = context;
            parameters[1] = callback;

            wifiPlugin.Call("startScanning", parameters);
        }

        public void OnSsidScanned(string ssid)
        {
            loading = false;

            if (!foundSsidList.Contains(ssid) && ssid.Length > 0)
            {
                foundSsidList.Add(ssid);
                Debug.Log($"SSIDList: Scanned Ssid : {ssid}");
            }

            ssidList.SetItems(foundSsidList.ToArray());
            ssidList.onOpen?.Invoke();
            ssidList.interactable = true;
            setBtn.interactable = true;
        }

        public void SetBluetooth()
        {
            status.SetActive(true);
            StartCoroutine(SetClient());
        }

        IEnumerator SetClient()
        {
            yield return new WaitUntil(() => selectedLamps.Where(i => i.selected).Count() == selectedLamps.Where(i => i.connected).Count());

            var ssid = ssidListObj.activeSelf ? ssidList.selected : ssidField.text;
            var password = passwordField.text;

            foreach (var lamp in selectedLamps.Where(l => l.connected))
            {
                var package = VoyagerNetworkMode.Client(ssid, password, lamp.peripheral.name);
                Debug.Log("json: " + Encoding.UTF8.GetString(package.ToData()));

                while (!selectedLamps.FirstOrDefault(l => l.peripheral.id == lamp.peripheral.id).writeReceived)
                {
                    lamp.Write(package.ToData());
                    yield return new WaitForSeconds(2);
                }

                lamp.settingClient = false;
            }

            GetComponentInParent<InspectorMenuContainer>().ShowMenu(null);
        }

        #endregion

        public void Set()
        {
            var ssid = ssidListObj.activeSelf ? ssidList.selected : ssidField.text;
            var password = passwordField.text;

            if (password.Length >= 8 && ssid.Length != 0 || password.Length == 0 && ssid.Length != 0)
            {
                var client = NetUtils.VoyagerClient;

                foreach (var lamp in WorkspaceUtils.SelectedLamps)
                    client.TurnToClient(lamp, ssid, password);

                GetComponentInParent<InspectorMenuContainer>().ShowMenu(null);
            }
            else if (ssid.Length == 0)
            {
                DialogBox.Show(
                    "INVALID SSID",
                    "SSID cannot be empty.",
                    new string[] { "OK" },
                    new Action[] { null }
                );
            }
            else
            {
                DialogBox.Show(
                    "INVALID PASSWORD",
                    "Password length must be at least 8 characters or empty for public WiFi networks.",
                    new string[] { "OK" },
                    new Action[] { null }
                );
            }
        }
    }
    class AndroidPluginCallBack : AndroidJavaProxy
    {
        internal Action<string> _callback;
        public AndroidPluginCallBack() : base("com.example.wifiplugin.PluginCallBack") { }

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

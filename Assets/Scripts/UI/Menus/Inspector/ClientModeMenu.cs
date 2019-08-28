using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Lamps;
using VoyagerApp.Utilities;

namespace VoyagerApp.UI.Menus
{
    public class ClientModeMenu : Menu
    {
        [SerializeField] float ssidPollTimeout      = 10.0f;
        [SerializeField] Text infoText              = null;
        [SerializeField] Toggle ssidsToggle         = null;
        [SerializeField] Dropdown ssidsDropdown     = null;
        [SerializeField] Toggle ssidToggle          = null;
        [SerializeField] InputField ssidField       = null;
        [SerializeField] InputField passwordField   = null;
        [SerializeField] Button setButton           = null;

        bool gotSsids;

        internal override void OnShow()
        {
            ssidsToggle.onValueChanged.AddListener(SsidsToggleChanged);
        }

        internal override void OnHide()
        {
            StopCoroutine(IEnumGetSsidListFromLamps());
        }

        #region Lamp SSIDS

        void SsidsToggleChanged(bool value)
        {
            if (value && (!gotSsids || ssidsDropdown.options.Count < 2))
            {
                ssidsDropdown.interactable = false;
                ssidsDropdown.options.Clear();
                var loading = new List<string>(new string[] { "Loading" });
                ssidsDropdown.AddOptions(loading);
                StartCoroutine(IEnumGetSsidListFromLamps());
            }
        }

        IEnumerator IEnumGetSsidListFromLamps()
        {
            List<List<string>> allSsids = new List<List<string>>();
            int count = WorkspaceUtils.SelectedLamps.Count;
            int gathered = 0;

            foreach (var lamp in WorkspaceUtils.SelectedLamps)
                PollSsidsFromLamp(lamp);

            void PollSsidsFromLamp(Lamp lamp)
            {
                NetUtils.VoyagerClient.GetSsidListFromLamp(
                    lamp,
                    OnSsidsReceived,
                    ssidPollTimeout);
            }

            void OnSsidsReceived(string[] ssids)
            {
                foreach (var ssid in ssids) Debug.Log(ssid);
                allSsids.Add(ssids.ToList());
                gathered++;
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
        }

        void OnSsidListReceived(List<string> ssids)
        {
            if (ssids.Count > 0)
            {
                gotSsids = true;
                ssidsDropdown.ClearOptions();
                ssidsDropdown.AddOptions(ssids);
                ssidsDropdown.interactable = ssidsToggle.isOn;
            }
            else
            {
                ssidsDropdown.options.Clear();
                var loading = new List<string>(new string[] { "Not found" });
                ssidsDropdown.AddOptions(loading);
            }
        }

        #endregion

        public void Set()
        {
            var client = NetUtils.VoyagerClient;
            var ssid = GetSsid();
            var password = passwordField.text;

            foreach (var lamp in WorkspaceUtils.SelectedLamps)
                client.TurnToClient(lamp, ssid, password);

            GetComponentInParent<InspectorMenuContainer>().ShowMenu(null);
        }

        string GetSsid()
        {
            if (gotSsids && ssidsToggle.isOn)
                return ssidsDropdown.options[ssidsDropdown.value].text;

            return ssidField.text;
        }
    }
}
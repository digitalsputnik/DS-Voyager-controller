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
        [SerializeField] GameObject ssidListObj     = null;
        [SerializeField] ListPicker ssidList        = null;
        [SerializeField] Button ssidRefreshBtn      = null;
        [SerializeField] GameObject ssidFieldObj    = null;
        [SerializeField] InputField ssidField       = null;
        [SerializeField] InputField passwordField   = null;
        [SerializeField] Button setBtn              = null;
        [SerializeField] string[] loadingAnim       = null;
        [SerializeField] float animationSpeed       = 0.6f;

        Dictionary<Lamp, List<string>> lampToSsids = new Dictionary<Lamp, List<string>>();
        bool loading;

        public override void Start()
        {
            base.Start();
            TypeSsidBtnClick();
        }

        internal override void OnShow()
        {
            ssidField.onValueChanged.AddListener(SsidFieldTextChanged);
            if (ssidFieldObj.activeSelf) TypeSsidBtnClick();
        }

        internal override void OnHide()
        {
            ssidField.onValueChanged.RemoveListener(SsidFieldTextChanged);
            lampToSsids.Clear();
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
            ssidListObj.gameObject.SetActive(false);
            ssidFieldObj.gameObject.SetActive(true);
            if (WorkspaceUtils.SelectedVoyagerLamps.Count > 0 && string.IsNullOrEmpty(ssidField.text))
                ssidField.text = WorkspaceUtils.SelectedVoyagerLamps[0].activePattern;
        }

        void SsidFieldTextChanged(string text)
        {
            setBtn.gameObject.SetActive(ssidField.text.Length >= 8);
        }

        #region Lamp SSIDS

        void StartLoading()
        {
            ssidList.index = 0;
            ssidList.interactable = false;
            ssidRefreshBtn.interactable = false;
            lampToSsids.Clear();
            StartCoroutine(IEnumGetSsidListFromLamps());
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

        public void Set()
        {
            var client = NetUtils.VoyagerClient;
            var ssid = ssidListObj.activeSelf ? ssidList.selected : ssidField.text;
            var password = passwordField.text;

            foreach (var lamp in WorkspaceUtils.SelectedLamps)
                client.TurnToClient(lamp, ssid, password);

            GetComponentInParent<InspectorMenuContainer>().ShowMenu(null);
        }
    }
}
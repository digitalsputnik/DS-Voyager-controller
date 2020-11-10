using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DigitalSputnik.Voyager;
using UnityEngine;
using UnityEngine.UI;
using VoyagerController.Workspace;

namespace VoyagerController.UI
{
    public class ClientModeMenu : Menu
    {
        private const float SSID_POLL_TIMEOUT = 15.0f;
        private const float LOADING_ANIMATION_SPEED = 0.6f;
        private static readonly string[] LOADING_ANIMATION_VALUES = new string[]
        {
            "Loading   ",
            "Loading.  ",
            "Loading.. ",
            "Loading...",
        };

        [SerializeField] private ListPicker _ssidList = null;
        [SerializeField] private InputField _ssidField = null;
        [SerializeField] private InputField _passwordField = null;
        [SerializeField] private Button _refreshButton = null;
        [SerializeField] private Button _setButton = null;

        private readonly Dictionary<VoyagerLamp, List<string>> _lampToSsids = new Dictionary<VoyagerLamp, List<string>>();
        private bool _loading;

        public override void Start()
        {
            base.Start();
            TypeSsidMode();
        }

        public void TypeSsidMode()
        {
            if (!_loading)
            {
                _ssidList.gameObject.SetActive(false);
                _ssidField.gameObject.SetActive(true);

                if (WorkspaceSelection.GetSelected<VoyagerItem>().Any() && string.IsNullOrEmpty(_ssidField.text))
                {
                    // TODO: Set ssid to one of the lamps active ssid || take the ssid from settings.
                    // _ssidField.text = WorkspaceSelection.GetSelected<VoyagerItem>().First().LampHandle
                }
            }
        }

        public void ScanSsidMode()
        {
            _ssidList.gameObject.SetActive(true);
            _ssidField.gameObject.SetActive(false);
            StartPolling();
        }

        public void Set()
        {
            var ssid = _ssidList.gameObject.activeSelf ? _ssidList.Selected : _ssidField.text;
            var password = _passwordField.text;

            if (password.Length >= 8 && ssid.Length != 0 || password.Length == 0 && ssid.Length != 0)
            {
                foreach (var voyager in WorkspaceSelection.GetSelected<VoyagerItem>().Select(i => i.LampHandle))
                {
                    // TODO: Implement network settings on library side!
                }

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

        internal override void OnShow()
        {
            if (_ssidField.gameObject.activeSelf) TypeSsidMode();
        }

        internal override void OnHide()
        {
            _lampToSsids.Clear();
        }
        
        #region Polling SSIDS

        private void StartPolling()
        {
            _ssidList.Index = 0;
            _ssidList.Interactable = false;
            _refreshButton.interactable = false;
            
            _lampToSsids.Clear();

            StartCoroutine(EnumPollSsidListFromLamps());
            StartCoroutine(EnumLoadingAnimation());
        }

        private IEnumerator EnumPollSsidListFromLamps()
        {
            yield return new WaitForSeconds(5.0f);
            OnSsidListReceived(new List<string>());
        }

        private void OnSsidListReceived(List<string> ssids)
        {
            if (ssids.Count > 0)
            {
                _ssidList.SetItems(ssids.ToArray());
                _ssidList.Interactable = true;
                _setButton.interactable = true;
                _refreshButton.interactable = true;
            }
            else
            {
                _ssidList.SetItems("Not found");
            }
        }

        private IEnumerator EnumLoadingAnimation()
        {
            var i = 0;
            while (_loading)
            {
                _ssidList.SetItems(LOADING_ANIMATION_VALUES[i]);
                yield return new WaitForSeconds(LOADING_ANIMATION_SPEED);
                if (++i >= LOADING_ANIMATION_VALUES.Length) i = 0;
            }
        }
        
        #endregion
    }
}
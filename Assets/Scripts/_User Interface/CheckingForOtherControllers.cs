using DigitalSputnik;
using DigitalSputnik.Networking;
using DigitalSputnik.Voyager;
using DigitalSputnik.Voyager.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;
using VoyagerController.UI;

namespace VoyagerApp.UI
{
    public class CheckingForOtherControllers : MonoBehaviour
    {
        #region Singleton
        static CheckingForOtherControllers instance;
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
                Destroy(this);
        }
        #endregion

        List<string> alertedControllers = new List<string>();

        void Start()
        {
            LampManager.Instance.GetClient<VoyagerNetworkClient>().OnMessageReceived += VoyagerClientMessageReceived;
            LampManager.Instance.GetClient<VoyagerNetworkClient>().OnConnectionChanged += VoyagerClientConnectionChanged;
        }

        void OnDestroy()
        {
            //LampManager.Instance.GetClient<VoyagerNetworkClient>().OnMessageReceived -= VoyagerClientMessageReceived;
            //LampManager.Instance.GetClient<VoyagerNetworkClient>().OnConnectionChanged -= VoyagerClientConnectionChanged;
        }

        void VoyagerClientConnectionChanged()
        {
            alertedControllers.Clear();
        }

        void VoyagerClientMessageReceived(object sender, byte[] data)
        {
            var endpoint = (IPEndPoint)sender;
            var json = Encoding.UTF8.GetString(data);
            var op = Packet.GetOpCode(json);

            if (op == OpCode.PollRequest)
                HandlePollPackage(endpoint);
        }

        void HandlePollPackage(IPEndPoint sender)
        {
            var senderIpStr = sender.Address.ToString();
            var selfAddresses = NetUtils.LocalIPAddresses;

            if (Application.platform == RuntimePlatform.IPhonePlayer &&
                alertedControllers.Count == 0)
                alertedControllers.Add(senderIpStr);

            foreach (var address in selfAddresses)
                RememberSelfAddress(address.ToString());

            if (!selfAddresses.Any(a => a.ToString() == senderIpStr))
                AnotherControllerDetected(senderIpStr);
        }

        void RememberSelfAddress(string address)
        {
            if (!alertedControllers.Contains(address))
                alertedControllers.Add(address);
        }

        void AnotherControllerDetected(string address)
        {
            if (!alertedControllers.Contains(address))
            {
                AlertAboutAnotherController();
                alertedControllers.Add(address);
            }
        }

        void AlertAboutAnotherController()
        {
            DialogBox.Show(
                "ALERT!",
                "Another DS Voyager Controller is detected from network. " +
                "We do not support multicontrolling.",
                new string[] { "EXIT", "OK" },
                new Action[] { ExitClicked, OkClicked }
            );
        }

        void ExitClicked()
        {
            Application.Quit();
        }

        void OkClicked()
        {

        }
    }
}

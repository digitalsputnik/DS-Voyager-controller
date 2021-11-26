using DigitalSputnik;
using DigitalSputnik.Voyager;
using System;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

namespace VoyagerController.UI
{
    public class AboutMenu : Menu
    {
        [SerializeField] private Text _versionText = null;
        [SerializeField] private Text _copyrightText = null;
        [SerializeField] private Text _updateText = null;

        internal override void OnShow()
        {
            _versionText.text = "VERSION " + Application.version + "\nLAMP VERSION " + VoyagerUpdater.Version;
            _copyrightText.text = $"© {DateTime.Now.Year} - Digital Sputnik";
        }

        public void OpenHelp()
        {
            Application.OpenURL(ApplicationSettings.HELP_URL);
        }

        public void ForceSelectUpdate()
        {
            try
            {
                var lamp = new VoyagerLamp();
                lamp.Serial = "Master Lamp";
                var endpoint = new LampNetworkEndPoint(IPAddress.Parse("172.20.0.1"));
                lamp.Endpoint = endpoint;
                VoyagerUpdater.UpdateLamp(lamp, OnUpdateFinished, OnUpdateMessage);
                _updateText.gameObject.SetActive(true);
            }
            catch (Exception ex)
            {
                DialogBox.Show(
                    "ERROR UPLOADING UPDATE",
                    ex.Message,
                    new string[] { "OK" },
                    new Action[] { null }
                );

                MainThread.Dispatch(() =>
                {
                    if (_updateText != null)
                        _updateText.text = "Lamp update failed";
                });
            }
        }

        private void OnUpdateFinished(VoyagerUpdateResponse response)
        {
            MainThread.Dispatch(() =>
            {
                if (_updateText != null)
                    _updateText.text = "Lamp successfully updated";
            });
        }

        private void OnUpdateMessage(VoyagerUpdateMessage message)
        {
            MainThread.Dispatch(() =>
            {
                if (_updateText != null)
                    _updateText.text = $"{message.Lamp.Serial}\n" +
                                       $"{message.Message.ToUpper()}";
            });
        }
    }
}
using DigitalSputnik.Videos;
using Klak.Spout;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VoyagerController.Effects;
using VoyagerController.Mapping;
using VoyagerController.Workspace;

namespace VoyagerController.UI
{
    public class EffectMappingMenu : Menu
    {
        private const string SELECT_ALL_TEXT = "SELECT ALL";
        private const string DESELECT_ALL_TEXT = "DESELECT ALL";
        
        [SerializeField] private GameObject _selectDeselectAllBtn = null;
        [SerializeField] ListPicker _syphonSource = null;
        [SerializeField] ListPicker _spoutSource = null;
        [SerializeField] IntField _streamDelayField = null;
        [SerializeField] IntField _streamFpsField = null;
        [SerializeField] GameObject _splitter = null;

        private Text _selectDeselectText;
        private Effect _effect;

        public override void Start()
        {
            _selectDeselectText = _selectDeselectAllBtn.GetComponentInChildren<Text>();
            UpdateUserInterface();
            base.Start();
        }
        
        internal override void OnShow()
        {
            WorkspaceSelection.SelectionChanged += UpdateUserInterface;
            _spoutSource.Opened.AddListener(OnSpoutSourceOpened);
            _spoutSource.Changed.AddListener(OnSpoutSourceChanged);
            _syphonSource.Opened.AddListener(OnSyphonSourceOpened);
            _syphonSource.Changed.AddListener(OnSyphonSourceChanged);
            _streamDelayField.OnChanged += StreamDelayChanged;
        }

        internal override void OnHide()
        {
            WorkspaceSelection.SelectionChanged -= UpdateUserInterface;
            _spoutSource.Opened.RemoveListener(OnSpoutSourceOpened);
            _spoutSource.Changed.RemoveListener(OnSpoutSourceChanged);
            _syphonSource.Opened.RemoveListener(OnSyphonSourceOpened);
            _syphonSource.Changed.RemoveListener(OnSyphonSourceChanged);
            _streamDelayField.OnChanged -= StreamDelayChanged;
        }

        public void SetEffect(Effect effect)
        {
            _effect = effect;

            if (effect is VideoEffect || effect is ImageEffect)
            {
                _syphonSource.transform.parent.gameObject.SetActive(false);
                _spoutSource.transform.parent.gameObject.SetActive(false);
                _streamDelayField.gameObject.SetActive(false);
                _splitter.SetActive(false);
            }
            else if (effect is SyphonEffect syphon)
            {
                SetupSyphonSourceList();
                _streamDelayField.SetValue(Convert.ToInt32(syphon.Delay * 1000.0));
                _syphonSource.transform.parent.gameObject.SetActive(true);
                _spoutSource.transform.parent.gameObject.SetActive(false);
                _streamDelayField.gameObject.SetActive(true);
                _splitter.SetActive(true);
            }
            else if (effect is SpoutEffect spout)
            {
                SetupSpoutSourceList();
                _streamDelayField.SetValue(Convert.ToInt32(spout.Delay * 1000.0));
                _syphonSource.transform.parent.gameObject.SetActive(false);
                _spoutSource.transform.parent.gameObject.SetActive(true);
                _streamDelayField.gameObject.SetActive(true);
                _splitter.SetActive(true);
            }
        }

        #region Syphon
        void SetupSyphonSourceList()
        {
            SyphonEffectLoader.RefreshClients(() =>
            {
                List<string> syphonItems = new List<string>();

                foreach (var server in SyphonEffectLoader.AvailableServers)
                    syphonItems.Add(server.Server + " - " + server.Application);

                _syphonSource.SetItems(syphonItems.ToArray());
                _syphonSource.Index = 0;
                _spoutSource.Interactable = true;
            });
        }

        void OnSyphonSourceOpened()
        {
            SyphonEffectLoader.RefreshClients(() =>
            {
                List<string> syphonItems = new List<string>();

                foreach (var server in SyphonEffectLoader.AvailableServers)
                    syphonItems.Add(server.Server + " - " + server.Application);

                _syphonSource.SetItems(syphonItems.ToArray());
            });
        }

        void OnSyphonSourceChanged()
        {
            if (_effect is SyphonEffect syphon)
            {
                syphon.Server = SyphonEffectLoader.AvailableServers[_syphonSource.Index];
                EffectManager.InvokeEffectModified(syphon);
            }
        }
        #endregion

        #region Spout
        void SetupSpoutSourceList()
        {
            SpoutEffectLoader.RefreshSources(() =>
            {
                _spoutSource.SetItems(SpoutEffectLoader.AvailableSources);
                _spoutSource.Index = 0;
                _spoutSource.Interactable = true;
            });
        }

        void OnSpoutSourceChanged()
        {
            if (_effect is SpoutEffect spout)
            {
                spout.Source = _spoutSource.Selected;
                EffectManager.InvokeEffectModified(spout);
            }
        }

        void OnSpoutSourceOpened()
        {
            SpoutEffectLoader.RefreshSources(() => _spoutSource.SetItems(SpoutEffectLoader.AvailableSources));
        }
        #endregion

        private void StreamDelayChanged(int value)
        {
            switch (_effect)
            {
                case SyphonEffect syphon:
                    syphon.Delay = Convert.ToDouble(value) / 1000;
                    break;
                case SpoutEffect spout:
                    spout.Delay = Convert.ToDouble(value) / 1000;
                    break;
            }
        }

        public void SelectDeselect()
        {
            if (!WorkspaceUtils.AllLampsSelected)
                WorkspaceUtils.SelectAllLamps();
            else
                WorkspaceUtils.DeselectAllLamps();
        }
        
        public void ExitEffectMapping()
        {
            EffectMapper.LeaveEffectMapping();
        }
        
        private void UpdateUserInterface()
        {
            var all = WorkspaceUtils.AllLampsSelected;
            _selectDeselectText.text = all ? DESELECT_ALL_TEXT : SELECT_ALL_TEXT;
        }
    }
}
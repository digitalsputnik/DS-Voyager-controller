using System.Collections;
using System.Collections.Generic;
using Klak.Spout;
using Klak.Syphon;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VoyagerApp.Effects;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;

namespace VoyagerApp.UI.Menus
{
    public class EffectMappingMenu : Menu
    {
        [SerializeField] StreamMapper streamMapper = null;
        [SerializeField] Text selectDeselectBtnText = null;
        [SerializeField] ListPicker syphonSource = null;
        [SerializeField] ListPicker spoutSource = null;
        [SerializeField] IntField streamDelayField = null;
        [SerializeField] IntField streamFpsField = null;
        [SerializeField] GameObject splitter = null;
        [SerializeField] GameObject alignmentBtn = null;

        Effect effect;
        bool hasSyphonInitialized;
        bool hasSpoutInitialized;

        (string, string)[] syphonSources;
        string[] spoutSources;

        public void SetEffect(Effect effect)
        {
            StopAllCoroutines();

            this.effect = effect;

            if (effect is Video || effect is Effects.Image)
            {
                syphonSource.transform.parent.gameObject.SetActive(false);
                spoutSource.transform.parent.gameObject.SetActive(false);
                streamDelayField.gameObject.SetActive(false);
                splitter.SetActive(false);
            }
            else if (effect is SyphonStream syphon)
            {
                SetupSyphon(syphon);
                streamDelayField.SetValue(syphon.delay);
                syphonSource.transform.parent.gameObject.SetActive(true);
                spoutSource.transform.parent.gameObject.SetActive(false);
                streamDelayField.gameObject.SetActive(true);
                splitter.SetActive(true);
            }
            else if (effect is SpoutStream spout)
            {
                SetupSpout(spout);
                streamDelayField.SetValue(spout.delay);
                syphonSource.transform.parent.gameObject.SetActive(false);
                spoutSource.transform.parent.gameObject.SetActive(true);
                streamDelayField.gameObject.SetActive(true);
                splitter.SetActive(true);
            }
        }

        public void SelectDeselect()
        {
            if (!WorkspaceUtils.AllLampsSelected)
                WorkspaceUtils.SelectAll();
            else
                WorkspaceUtils.DeselectAll();
        }

        internal override void OnShow()
        {
            streamMapper.Delay = streamDelayField.Value;

            WorkspaceSelection.instance.onSelectionChanged += EnableDisableObjects;
            streamDelayField.onChanged += StreamDelayChanged;

            streamDelayField.gameObject.SetActive(!(effect is Video || effect is Effects.Image));

            EnableDisableObjects();
        }

        internal override void OnHide()
        {
            WorkspaceSelection.instance.onSelectionChanged -= EnableDisableObjects;
            streamDelayField.onChanged -= StreamDelayChanged;
            streamFpsField.onChanged -= StreamFpsChanged;
        }

        #region Syphon
        void SetupSyphon(SyphonStream syphon)
        {
            if (hasSyphonInitialized)
            {
                syphonSource.onChanged.RemoveListener(OnSyphonSourceChanged);
                syphonSource.onOpen.RemoveListener(OnSyphonSourceOpened);
            }

            syphonSources = SyphonHelper.GetListOfServers();

            if (syphonSources != null)
            {
                SetupSyphonSourceList(syphon);
                syphonSource.interactable = true;
            }
            else
                StartCoroutine(IEnumWaitForFirstSyphonSources(syphon));

            syphonSource.onChanged.AddListener(OnSyphonSourceChanged);
            syphonSource.onOpen.AddListener(OnSyphonSourceOpened);

            hasSyphonInitialized = true;
        }

        IEnumerator IEnumWaitForFirstSyphonSources(SyphonStream syphon)
        {
            syphonSource.SetItems();
            syphonSource.interactable = false;
            bool got = false;

            while (!got)
            {
                syphonSources = SyphonHelper.GetListOfServers();

                if (syphonSources != null)
                {
                    SetupSyphonSourceList(syphon);
                    syphonSource.interactable = true;
                    got = true;
                }

                yield return new WaitForSeconds(1.0f);
            }
        }

        void SetupSyphonSourceList(SyphonStream syphon)
        {
            var items = new string[syphonSources.Length];
            for (int i = 0; i < syphonSources.Length; i++)
            {
                var source = syphonSources[i];
                var names = new List<string>();

                if (!string.IsNullOrEmpty(source.Item1))
                    names.Add(source.Item1);

                if (!string.IsNullOrEmpty(source.Item2))
                    names.Add(source.Item2);

                items[i] = string.Join(" - ", names);
            }
            syphonSource.SetItems(items);
            syphon.server = syphonSources[syphonSource.index].Item1;
            syphon.application = syphonSources[syphonSource.index].Item2;
        }

        void OnSyphonSourceOpened()
        {
            SetupSyphonSourceList(effect as SyphonStream);
        }

        void OnSyphonSourceChanged()
        {
            SyphonStream syphon = (SyphonStream)effect;

            syphon.server = syphonSources[syphonSource.index].Item1;
            syphon.application = syphonSources[syphonSource.index].Item2;

            streamMapper.SetEffect(syphon);
        }
        #endregion

        #region Spout
        void SetupSpout(SpoutStream spout)
        {
            if (hasSpoutInitialized)
            {
                spoutSource.onChanged.RemoveListener(OnSpoutSourceChanged);
                spoutSource.onOpen.RemoveListener(OnSpoutSourceOpened);
            }

            spoutSources = SpoutManager.GetSourceNames();

            if (spoutSources.Length > 0)
            {
                SetupSpoutSourceList(spout);
                spout.source = spoutSources[0];
                streamMapper.SetEffect(spout);
            }
            else
            {
                StartCoroutine(WaitForFirstSpoutSource(spout));
            }

            spoutSource.onChanged.AddListener(OnSpoutSourceChanged);
            spoutSource.onOpen.AddListener(OnSpoutSourceOpened);
            hasSpoutInitialized = true;
        }

        IEnumerator WaitForFirstSpoutSource(SpoutStream spout)
        {
            spoutSource.SetItems();
            spoutSource.interactable = false;

            bool got = false;
            while (!got)
            {
                spoutSources = SpoutManager.GetSourceNames();

                if (spoutSources.Length > 0)
                {
                    SetupSpoutSourceList(spout);
                    spoutSource.interactable = true;
                    got = true;
                }

                yield return new WaitForSeconds(1.0f);
            }
        }

        void SetupSpoutSourceList(SpoutStream spout)
        {
            spoutSource.SetItems(spoutSources);
            spoutSource.index = 0;
            spoutSource.interactable = true;
        }

        void OnSpoutSourceChanged()
        {
            SpoutStream spout = (SpoutStream)effect;
            spout.source = spoutSources[spoutSource.index];
            streamMapper.SetEffect(spout);
        }

        void OnSpoutSourceOpened()
        {
            spoutSources = SpoutManager.GetSourceNames();
            spoutSource.SetItems(spoutSources);
        }
        #endregion

        private void StreamDelayChanged(int value)
        {
            if (effect is Stream stream)
            {
                streamMapper.Delay = streamDelayField.Value;
                stream.delay = value;
            }
        }

        private void StreamFpsChanged(int value)
        {
            streamMapper.Fps = streamFpsField.Value;
        }

        public void ReturnToWorkspace()
        {
            PlayerPrefs.SetInt("from_video_mapping", 1);
            SceneManager.LoadScene(0);
        }

        private void EnableDisableObjects()
        {
            var one = WorkspaceUtils.AtLastOneLampSelected;
            var all = WorkspaceUtils.AllLampsSelected;

            selectDeselectBtnText.text = all ? "DESELECT ALL" : "SELECT ALL";
            alignmentBtn.SetActive(one);
        }
    }
}
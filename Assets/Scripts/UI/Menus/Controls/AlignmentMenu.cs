using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;

namespace VoyagerApp.UI.Menus
{
    public class AlignmentMenu : Menu
    {
        [SerializeField] GameObject selectText = null;
        [SerializeField] GameObject selectDeselectBtn = null;
        [SerializeField] GameObject undoBtn = null;

        [Header("Alignment")]
        [SerializeField] GameObject alignTitle = null;
        [SerializeField] GameObject alignHorizontal = null;
        [SerializeField] GameObject alignVertical = null;
        [SerializeField] GameObject alignFlip = null;
        [SerializeField] GameObject alignScale = null;

        [Header("Distribute")]
        [SerializeField] GameObject distTitle = null;
        [SerializeField] GameObject distHorizontal = null;
        [SerializeField] GameObject distVertical = null;

        List<List<LampWorkspaceState>> _states = new List<List<LampWorkspaceState>>();

        public void EditColorFx()
        {
            SaveWorkspaceState();
            WorkspaceUtils.EnterToVideoMapping();
        }

        public void AlignHorizontally()
        {
            SaveWorkspaceState();
            WorkspaceUtils.AlignSelectedLampsHorizontally();
        }

        public void AlignVertically()
        {
            SaveWorkspaceState();
            WorkspaceUtils.AlignSelectedLampsVertically();
        }

        public void AlignFlip()
        {
            SaveWorkspaceState();
            WorkspaceUtils.FlipSelectedLamps();
        }

        public void AliignScale()
        {
            SaveWorkspaceState();
            WorkspaceUtils.ScaleSelectedLampsBasedOnBiggest();
        }

        public void DestributeHorizontally()
        {
            SaveWorkspaceState();
            WorkspaceUtils.DistributeSelectedLampsHorizontally();
        }

        public void DestributeVertically()
        {
            SaveWorkspaceState();
            WorkspaceUtils.DistributeSelectedLampsVertically();
        }

        public void SelectDeselect()
        {
            if (!WorkspaceUtils.AllLampsSelected)
                WorkspaceUtils.SelectAll();
            else
                WorkspaceUtils.DeselectAll();
        }

        public void Undo()
        {
            if (_states.Count > 0)
            {
                var states = _states[0];

                foreach (var lampItem in WorkspaceUtils.LampItems)
                {
                    bool selected = WorkspaceSelection.instance.Contains(lampItem);
                    var state = states.FirstOrDefault(s => s.lamp == lampItem.lamp);

                    if (state != null)
                    {
                        WorkspaceSelection.instance.DeselectItem(lampItem);
                        WorkspaceManager.instance.RemoveItem(lampItem);
                        var item = state.lamp.AddToWorkspace(state.position, state.scale, state.rotation);
                        if (selected) WorkspaceSelection.instance.SelectItem(item);
                    }
                }

                _states.RemoveAt(0);
            }
            DisableEnableItems();
        }

        internal override void OnShow()
        {
            DisableEnableItems();
            WorkspaceSelection.instance.onSelectionChanged += DisableEnableItems;
        }

        internal override void OnHide()
        {
            WorkspaceSelection.instance.onSelectionChanged -= DisableEnableItems;
            _states.Clear();
        }

        void DisableEnableItems()
        {
            bool one = WorkspaceUtils.AtLastOneLampSelected;
            bool all = WorkspaceUtils.AllLampsSelected;
            bool has = WorkspaceUtils.VoyagerLamps.Count != 0;

            selectDeselectBtn.SetActive(has);
            selectDeselectBtn
                .GetComponentInChildren<Text>()
                .text = all ? "DESELECT ALL" : "SELECT ALL";
            undoBtn.SetActive(_states.Count > 0);

            selectText.SetActive(!one);

            alignTitle.SetActive(one);
            alignHorizontal.SetActive(one);
            alignVertical.SetActive(one);
            alignFlip.SetActive(one);
            alignScale.SetActive(one);

            distTitle.SetActive(one);
            distHorizontal.SetActive(one);
            distVertical.SetActive(one);
        }

        void SaveWorkspaceState()
        {
            _states.Insert(0, WorkspaceUtils.LampStates());
            DisableEnableItems();
        }
    }
}

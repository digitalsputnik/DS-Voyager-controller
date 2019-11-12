﻿using UnityEngine;
using VoyagerApp.Utilities;
using VoyagerApp.Workspace;

namespace VoyagerApp.UI.Menus
{
    public class AlignmentMenu : Menu
    {
        [SerializeField] GameObject selectText = null;

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

        public void EditColorFx() => WorkspaceUtils.EnterToVideoMapping();
        public void AlignHorizontally() => WorkspaceUtils.AlignSelectedLampsHorizontally();
        public void AlignVertically() => WorkspaceUtils.AlignSelectedLampsVertically();
        public void AlignFlip() => WorkspaceUtils.FlipSelectedLamps();
        public void AliignScale() => WorkspaceUtils.ScaleSelectedLampsBasedOnBiggest();

        public void DestributeHorizontally() => WorkspaceUtils.DistributeSelectedLampsHorizontally();
        public void DestributeVertically() => WorkspaceUtils.DistributeSelectedLampsVertically();

        internal override void OnShow()
        {
            DisableEnableItems();
            WorkspaceSelection.instance.onSelectionChanged += DisableEnableItems;
        }

        internal override void OnHide()
        {
            WorkspaceSelection.instance.onSelectionChanged -= DisableEnableItems;
        }

        void DisableEnableItems()
        {
            bool one = WorkspaceUtils.AtLastOneLampSelected;

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
    }
}
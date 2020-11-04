﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VoyagerController.Workspace
{
    public class WorkspaceSelection : MonoBehaviour
    {
        #region Singleton
        private static WorkspaceSelection _instance;
        private void Awake() => _instance = this;
        #endregion

        public delegate void SelectionHandler();
        public static SelectionHandler SelectionChanged;

        private static IEnumerable<WorkspaceItem> Selected => _instance._selected;
        
        private readonly List<WorkspaceItem> _selected = new List<WorkspaceItem>();
        
        private void Start()
        {
            WorkspaceManager.ItemRemoved += ItemRemovedFromWorkspace;
        }

        private void OnDestroy()
        {
            WorkspaceManager.ItemRemoved -= ItemRemovedFromWorkspace;
        }

        public static IEnumerable<T> GetSelected<T>() where T : WorkspaceItem => Selected.OfType<T>();

        public static IEnumerable<WorkspaceItem> GetSelected() => GetSelected<WorkspaceItem>(); 

        public static void SelectItem(WorkspaceItem selectable)
        {
            if (Contains(selectable)) return;
            
            selectable.Select();
            _instance._selected.Add(selectable);
            SelectionChanged?.Invoke();
        }
        
        public static void DeselectItem(WorkspaceItem selectable)
        {
            if (!Contains(selectable)) return;
            
            selectable.Deselect();
            _instance._selected.Remove(selectable);
            SelectionChanged?.Invoke();
        }

        public static bool Contains(WorkspaceItem selectable) => Selected.Contains(selectable);
        
        // public void ReselectItem() => OnSelectionChanged?.Invoke();

        public static void Clear()
        {
            _instance._selected.ForEach(s => s.Deselect());
            _instance._selected.Clear();
            SelectionChanged?.Invoke();
        }

        private static void ItemRemovedFromWorkspace(WorkspaceItem item) => DeselectItem(item);
    }
}
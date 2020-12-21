﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VoyagerController.Workspace
{
    public class WorkspaceManager : MonoBehaviour
    {
        private const string WORKSPACE_ITEMS_PATH = "Workspace Items";
        
        private static WorkspaceManager _instance;
        
        public static WorkspaceItemHandler ItemAdded;
        public static WorkspaceItemHandler ItemRemoved;

        private WorkspaceItem[] _itemPrefabs;
        private readonly List<WorkspaceItem> _items = new List<WorkspaceItem>();

        private static float step = -2.0f;

        private void Awake()
        {
            _instance = this;
            _itemPrefabs = Resources.LoadAll<WorkspaceItem>(WORKSPACE_ITEMS_PATH);
        }

        public static Transform Transform => _instance.transform;

        public static IEnumerable<WorkspaceItem> GetItems() => GetItems<WorkspaceItem>();

        public static IEnumerable<T> GetItems<T>() where T : WorkspaceItem
        {
            return _instance._items.OfType<T>();
        }

        public static T InstantiateItem<T>(object data) where T : WorkspaceItem
        {
            if (!_instance._itemPrefabs.Any(i => i is T))
                return null;
            
            var prefab = _instance._itemPrefabs.FirstOrDefault(i => i is T) as T;
            var item = Instantiate(prefab, _instance.transform);

            item.Setup(data);

            if (item is VoyagerItem voyager)
            {
                voyager.transform.localPosition = new Vector2(
                    _instance._items.Count() != 0 ? _instance._items.Last().transform.localPosition.x : 0f,
                    _instance._items.Count() != 0 ? _instance._items.Last().transform.localPosition.y + step : 0f
                );

                var lampMetaData = Metadata.Get(voyager.LampHandle.Serial);
                lampMetaData.WorkspaceMapping.Position = new[] { voyager.transform.localPosition.x, voyager.transform.localPosition.y };
                lampMetaData.InWorkspace = true;
            }

            _instance._items.Add(item);

            ItemAdded?.Invoke(item);
            return item;
        }

        public static void RemoveItem<T>(T item) where T : WorkspaceItem
        {
            if (_instance._items.Contains(item))
            {
                Destroy(item.gameObject);
                _instance._items.Remove(item);

                if (item is VoyagerItem voyager)
                    Metadata.Get(voyager.LampHandle.Serial).InWorkspace = false;
                
                ItemRemoved?.Invoke(item);
            }
        }

        public static void Clear()
        {
            for (var i = _instance._items.Count - 1; i >= 0; i--)
                RemoveItem(_instance._items[i]);
        }
    }

    public delegate void WorkspaceItemHandler(WorkspaceItem item);
}
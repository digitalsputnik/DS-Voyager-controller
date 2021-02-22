using System.Collections.Generic;
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
                Metadata.GetLamp(voyager.LampHandle.Serial).InWorkspace = true;

            _instance._items.Add(item);

            ItemAdded?.Invoke(item);
            return item;
        }
        
        public static T InstantiateItem<T>(object data, string id) where T : WorkspaceItem
        {
            if (!_instance._itemPrefabs.Any(i => i is T))
                return null;
            
            var prefab = _instance._itemPrefabs.FirstOrDefault(i => i is T) as T;
            var item = Instantiate(prefab, _instance.transform);

            item.Setup(data, id);

            if (item is VoyagerItem voyager)
                Metadata.GetLamp(voyager.LampHandle.Serial).InWorkspace = true;

            _instance._items.Add(item);

            ItemAdded?.Invoke(item);
            return item;
        }

        public static T InstantiateItem<T>(object data, Vector3 position) where T : WorkspaceItem
        {
            var item = InstantiateItem<T>(data);
            var pos = position;
            var transform = item.transform;
            pos.z = transform.position.z;
            transform.position = pos;
            return item;
        }
        
        public static T InstantiateItem<T>(object data, Vector2 position, float scale) where T : WorkspaceItem
        {
            var item = InstantiateItem<T>(data, position);
            item.transform.localScale = Vector3.one * scale;
            return item;
        }

        public static T InstantiateItem<T>(object data, Vector2 position, float scale, float rotation) where T : WorkspaceItem
        {
            var item = InstantiateItem<T>(data, position, scale);
            item.transform.eulerAngles = new Vector3(0.0f, 0.0f, rotation);
            return item;
        }

        public static void RemoveItem<T>(T item) where T : WorkspaceItem
        {
            if (_instance._items.Contains(item))
            {
                Destroy(item.gameObject);
                _instance._items.Remove(item);

                if (item is VoyagerItem voyager)
                    Metadata.GetLamp(voyager.LampHandle.Serial).InWorkspace = false;
                
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
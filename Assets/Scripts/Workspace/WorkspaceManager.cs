using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VoyagerController.Workspace
{
    public class WorkspaceManager : MonoBehaviour
    {
        private const string WORKSPACE_ITEMS_PATH = "Workspace Items";
        
        private static WorkspaceManager _instance;
        
        public static WorkspaceItemHandler OnItemAdded;
        public static WorkspaceItemHandler OnItemRemoved;

        private WorkspaceItem[] _itemPrefabs;
        private readonly List<WorkspaceItem> _items = new List<WorkspaceItem>();

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

            _instance._items.Add(item);
            OnItemAdded?.Invoke(item);
                
            return item;
        }

        public static void RemoveItem<T>(T item) where T : WorkspaceItem
        {
            if (_instance._items.Contains(item))
            {
                Destroy(item.gameObject);
                _instance._items.Remove(item);
                OnItemRemoved?.Invoke(item);
            }
        }
    }

    public delegate void WorkspaceItemHandler(WorkspaceItem item);
}
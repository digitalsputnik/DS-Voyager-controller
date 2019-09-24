using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VoyagerApp.Workspace
{
    public class WorkspaceManager : MonoBehaviour
    {
        #region Singleton
        public static WorkspaceManager instance;
        void Awake() => instance = this;
        #endregion

        public event WorkspaceItemHandler onItemAdded;
        public event WorkspaceItemHandler onItemRemoved;

        [SerializeField]
        List<WorkspaceItemView> prefabs = new List<WorkspaceItemView>();
        List<WorkspaceItemView> items = new List<WorkspaceItemView>();

        public List<WorkspaceItemView> Items => items;

        public T InstantiateItem<T>(object data, Vector2 position, float scale, float rotation)
            where T : WorkspaceItemView
        {
            T item = InstantiateItem<T>(data, position, scale);
            item.transform.eulerAngles = new Vector3(0.0f, 0.0f, rotation);
            return item;
        }

        public T InstantiateItem<T>(object data, Vector2 position, float scale)
            where T : WorkspaceItemView
        {
            T item = InstantiateItem<T>(data, position);
            item.transform.localScale = Vector3.one * scale;
            return item;
        }

        public T InstantiateItem<T>(object data, Vector2 position)
            where T : WorkspaceItemView
        {
            T item = InstantiateItem<T>(data);
            Vector3 pos = position;
            pos.z = item.transform.position.z;
            item.transform.position = pos;
            return item;
        }

        public T InstantiateItem<T>(object data) where T : WorkspaceItemView
        {
            T prefab = GetPrefabOfType<T>();
            T item = Instantiate(prefab, transform);
            item.Setup(data);
            items.Add(item);
            onItemAdded?.Invoke(item);
            return item;
        }

        public void RemoveItem(WorkspaceItemView item)
        {
            if (item == null) return;

            foreach (var child in new List<WorkspaceItemView>(item.children))
            {
                child.SetParent(null);
                RemoveItem(child);
            }

            items.Remove(item);
            onItemRemoved?.Invoke(item);
            Destroy(item.gameObject);
        }

        T GetPrefabOfType<T>() where T : WorkspaceItemView
        {
            return (T)prefabs.FirstOrDefault(_ => _ is T);
        }

        public T[] GetItemsOfType<T>() where T : WorkspaceItemView
        {
            List<T> list = new List<T>();
            foreach (WorkspaceItemView item in items)
                if (item is T cast) list.Add(cast);
            return list.ToArray();
        }

        public WorkspaceItemView GetItem(string guid)
        {
            return Items.FirstOrDefault(_ => _.guid == guid);
        }


        public void Clear()
        {
            foreach (var item in new List<WorkspaceItemView>(items))
                RemoveItem(item);
        }
    }

    public delegate void WorkspaceItemHandler(WorkspaceItemView item);
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoyagerApp.Workspace
{
    public abstract class WorkspaceItemView : MonoBehaviour
    {
        [SerializeField] Transform childContainer = null;

        public string guid;

        public ItemMove move { get; private set; }

        public Vector2 position => transform.position;
        public float scale => transform.lossyScale.x;
        public float rotation => transform.eulerAngles.z;

        public List<WorkspaceItemView> children = new List<WorkspaceItemView>();
        public WorkspaceItemView parent { get; private set; }

        public virtual void Setup(object data)
        {
            Generate();
            guid = Guid.NewGuid().ToString();
            move = GetComponentInChildren<ItemMove>();
        }

        public virtual WorkspaceItemSaveData ToData()
        {
            return new WorkspaceItemSaveData
            {
                guid = guid,
                x = position.x,
                y = position.y,
                scale = scale,
                rotation = rotation,
                parentguid = parent == null ? "" : parent.guid
            };
        }

        public void SetParent(WorkspaceItemView item)
        {
            if (parent != null)
            {
                if (parent.children.Contains(this))
                    parent.RemoveChild(this);
            }

            if (item != null)
                item.SetChild(this);
        }

        void SetChild(WorkspaceItemView item)
        {
            item.parent = this;
            item.transform.SetParent(childContainer);
            children.Add(item);
        }

        void RemoveChild(WorkspaceItemView item)
        {
            item.parent = null;
            item.transform.SetParent(WorkspaceManager.instance.transform);
            children.Remove(item);
        }

        

        protected virtual void Generate() { }
    }
}

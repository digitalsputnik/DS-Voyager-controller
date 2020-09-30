using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoyagerController.Workspace
{
    public class WorkspaceItem : MonoBehaviour
    {
        [SerializeField] private Transform _childContainer;

        public virtual bool Selectable { get; } = true;
        public string Uid { get; private set; }
        public List<WorkspaceItem> Children = new List<WorkspaceItem>();
        public WorkspaceItem Parent { get; private set; }
        
        public virtual bool Setup(object data, string uid = "")
        {
            Generate();
            Uid = string.IsNullOrEmpty(uid) ? Guid.NewGuid().ToString() : uid;
            return true;
        }
        
        public virtual void Select() { }
        
        public virtual void Deselect() { }
        
        public virtual Bounds Bounds { get; }
        
        public virtual Vector3[] SelectPositions { get; }

        protected virtual void Generate() { }
        
        public void SetParent(WorkspaceItem item)
        {
            if (Parent != null)
            {
                if (Parent.Children.Contains(this))
                    Parent.RemoveChild(this);
            }

            if (item != null)
                item.SetChild(this);
        }

        private void SetChild(WorkspaceItem item)
        {
            item.Parent = this;
            item.transform.SetParent(_childContainer);
            Children.Add(item);
        }

        private void RemoveChild(WorkspaceItem item)
        {
            item.Parent = null;
            item.transform.SetParent(WorkspaceManager.Transform);
            Children.Remove(item);
        }
    }
}
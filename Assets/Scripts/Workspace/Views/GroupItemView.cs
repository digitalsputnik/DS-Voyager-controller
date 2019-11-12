using System;
using UnityEngine;

namespace VoyagerApp.Workspace.Views
{
    public class GroupItemView : ItemsContainerView
    {
        const int WIDTH = 7;
        const int HEIGHT = 4;

        [SerializeField] new MeshRenderer renderer = null;
        [SerializeField] Color color = Color.white;
        [SerializeField] TextMesh nameText = null;

        public override void Setup(object data)
        {
            nameText.text = (string)data;
            base.Setup(data);
        }

        protected override void Generate()
        {
            Vector2 size = new Vector2(WIDTH, HEIGHT);
            renderer.transform.localScale = size;

            Vector2 outlineSize = Vector2.one * outlineThickness;
            outline.transform.localScale = size + outlineSize;

            renderer.material.color = color;
        }

        public override WorkspaceItemSaveData ToData()
        {
            return new GroupItemSaveData
            {
                guid = guid,
                x = position.x,
                y = position.y,
                scale = scale,
                rotation = rotation,
                parentguid = parent == null ? "" : parent.guid,
                name = nameText.text
            };
        }
    }

    [Serializable]
    public class GroupItemSaveData : WorkspaceItemSaveData
    {
        public string name;

        public override WorkspaceItemView Load()
        {
            var manager = WorkspaceManager.instance;
            var item = manager.InstantiateItem<GroupItemView>(name,
                                                              position,
                                                              scale,
                                                              rotation);
            item.guid = guid;
            return item;
        }
    }
}
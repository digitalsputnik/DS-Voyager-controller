using System;
using UnityEngine;

namespace VoyagerApp.Workspace.Views
{
    public class PictureItemView : ItemsContainerView
    {
        [SerializeField] new MeshRenderer renderer = null;
        [SerializeField] float pixelsPerUnit = 20;

        Texture2D picture;

        public override void Setup(object data)
        {
            picture = (Texture2D)data;
            base.Setup(data);
        }

        protected override void Generate()
        {
            Vector2 size = new Vector2(picture.width, picture.height) / pixelsPerUnit;
            renderer.transform.localScale = size;

            Vector2 outlineSize = Vector2.one * outlineThickness;
            outline.transform.localScale = size + outlineSize;

            renderer.material.mainTexture = picture;
        }

        public override WorkspaceItemSaveData ToData()
        {
            return new PictureItemSaveData
            {
                guid = guid,
                x = position.x,
                y = position.y,
                scale = scale,
                rotation = rotation,
                parentguid = parent == null ? "" : parent.guid,
                image = picture
            };
        }
    }

    [Serializable]
    public class PictureItemSaveData : WorkspaceItemSaveData
    {
        public Texture2D image;

        public override void Load()
        {
            var manager = WorkspaceManager.instance;
            var item = manager.InstantiateItem<PictureItemView>(image,
                                                                position,
                                                                scale,
                                                                rotation);
            item.guid = guid;
        }
    }
}
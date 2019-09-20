using System;
using UnityEngine;
using VoyagerApp.Utilities;

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

        public void PositionBasedCamera()
        {
            SetupMeshSize();
            SetupOutlineSize();

            Vector3 pos = transform.position;
            pos.x = Camera.main.transform.position.x;
            pos.y = Camera.main.transform.position.y;
            transform.position = pos;
        }

        void SetupMeshSize()
        {
            Vector2 maxScale = CalculateMeshMaxScale();

            float maxScaleAspect = maxScale.x / maxScale.y;
            float videoAspect = (float)picture.width / picture.height;

            Vector2 s = maxScale;

            if (videoAspect > maxScaleAspect)
                s.y = maxScale.y / videoAspect * maxScaleAspect;
            else if (videoAspect < maxScaleAspect)
                s.x = maxScale.x / maxScaleAspect * videoAspect;

            renderer.transform.localScale = s;
        }

        Vector2 CalculateMeshMaxScale()
        {
            Vector2 screenWorldSize = VectorUtils.ScreenSizeWorldSpace;
            float width = screenWorldSize.x * 0.8f;
            float height = screenWorldSize.y - screenWorldSize.x * 0.2f;
            return new Vector2(width, height);
        }

        void SetupOutlineSize()
        {
            Vector2 outlineSize = Vector2.one * outlineThickness;
            Vector2 pictureSize = renderer.transform.localScale;
            outline.transform.localScale = pictureSize + outlineSize;
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
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using VoyagerApp.Utilities;

namespace VoyagerApp.Workspace.Views
{
    public class PictureItemView : ItemsContainerView, ISelectableItem
    {
        static List<PictureItemView> orderQueue = new List<PictureItemView>();

        [SerializeField] new MeshRenderer renderer = null;
        [SerializeField] float pixelsPerUnit = 20;
        [SerializeField] Color selectedColor = Color.yellow;
        [SerializeField] Color deselectedColor = Color.grey;

        public Texture2D picture;

        public bool Selected { get; private set; }
        public Bounds Bounds => renderer.bounds;
        public WorkspaceItemView View => this;

        public int GetOrder()
        {
            return orderQueue.IndexOf(this);
        }

        public float3[] SelectPositions
        {
            get
            {
                float3[] positions = new float3[5];
                positions[0] = transform.position;
                positions[1] = renderer.transform.TransformPoint(new Vector3(-0.5f,  0.5f, 0.0f));
                positions[2] = renderer.transform.TransformPoint(new Vector3( 0.5f,  0.5f, 0.0f));
                positions[3] = renderer.transform.TransformPoint(new Vector3(-0.5f, -0.5f, 0.0f));
                positions[4] = renderer.transform.TransformPoint(new Vector3( 0.5f, -0.5f, 0.0f));
                return positions;
            }
        }

        public override void Setup(object data)
        {
            picture = (Texture2D)data;
            deselectedColor = outline.GetComponent<MeshRenderer>().material.color;
            base.Setup(data);

            orderQueue.Add(this);
            OrderQueueChanged();

            WorkspaceSelection.instance.onSelectionChanged += SelectionChanged;
        }

        void OnDestroy()
        {
            orderQueue.Remove(this);
            OrderQueueChanged();
            WorkspaceSelection.instance.onSelectionChanged -= SelectionChanged;
        }

        static void OrderQueueChanged()
        {
            for (int i = 0; i < orderQueue.Count; i++)
            {
                var view = orderQueue[i];
                view.renderer.material.renderQueue = 2000 + i;
            }
        }

        void SelectionChanged()
        {
            foreach (var item in WorkspaceSelection.instance.Selected)
            {
                if (item is PictureItemView pictureView)
                    pictureView.MoveToLast();
            }
        }

        void MoveToLast()
        {
            orderQueue.Remove(this);
            orderQueue.Add(this);
            OrderQueueChanged();
        }

        protected override void Generate()
        {
            Vector2 size = new Vector2(picture.width, picture.height) / pixelsPerUnit;
            size *= renderer.transform.localScale;
            renderer.transform.localScale = size;

            Vector2 outlineSize = Vector2.one * outlineThickness;
            outline.transform.localScale = size + outlineSize;

            renderer.material.SetTexture("_BaseMap", picture);
            //renderer.material.mainTexture = picture;
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

        public void SetOrder(int index)
        {
            orderQueue.Remove(this);
            orderQueue.Insert(index, this);
            OrderQueueChanged();
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

            float diff = renderer.transform.localScale.x / s.x;
            transform.localScale = transform.localScale / diff;
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
                image = picture,
                queueIndex = orderQueue.IndexOf(this)
            };
        }

        public void Select()
        {
            MeshRenderer outRend = outline.GetComponent<MeshRenderer>();
            outRend.material.color = selectedColor;
            Selected = true;
        }

        public void Deselect()
        {
            MeshRenderer outRend = outline.GetComponent<MeshRenderer>();
            outRend.material.color = deselectedColor;
            Selected = false;
        }
    }

    [Serializable]
    public class PictureItemSaveData : WorkspaceItemSaveData
    {
        public Texture2D image;
        public int queueIndex;

        public override WorkspaceItemView Load()
        {
            var manager = WorkspaceManager.instance;
            var item = manager.InstantiateItem<PictureItemView>(image,
                                                                position,
                                                                scale,
                                                                rotation);
            item.guid = guid;
            return item;
        }
    }
}

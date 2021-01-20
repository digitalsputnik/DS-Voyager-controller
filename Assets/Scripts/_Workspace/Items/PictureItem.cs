using System.Collections.Generic;
using DigitalSputnik;
using DigitalSputnik.Colors;
using Unity.Mathematics;
using UnityEngine;

namespace VoyagerController.Workspace
{
    public class PictureItem : WorkspaceItem
    {
        public Texture2D Picture;
        public int Order { get; set; } = 0;

        [Header("Renderer")]
        [SerializeField] MeshRenderer _renderer = null;
        [SerializeField] float pixelsPerUnit = 20;

        [Header("Outline")]
        [SerializeField] private Transform _outline = null;
        [SerializeField] private float _outlineThickness = 0.2f;

        public override Vector3[] SelectPositions
        {
            get
            {
                Vector3[] positions = new Vector3[5];
                positions[0] = transform.position;
                positions[1] = _renderer.transform.TransformPoint(new Vector3(-0.5f, 0.5f, 0.0f));
                positions[2] = _renderer.transform.TransformPoint(new Vector3(0.5f, 0.5f, 0.0f));
                positions[3] = _renderer.transform.TransformPoint(new Vector3(-0.5f, -0.5f, 0.0f));
                positions[4] = _renderer.transform.TransformPoint(new Vector3(0.5f, -0.5f, 0.0f));
                return positions;
            }
        }

        public override Bounds Bounds => _renderer.bounds;

        public override bool Setup(object data, string uid = "")
        {
            Picture = data as Texture2D;

            if (Picture == null) return false;

            return base.Setup(data, uid);
        }

        protected override void Generate()
        {
            Vector2 size = new Vector2(Picture.width, Picture.height) / pixelsPerUnit;
            size *= _renderer.transform.localScale;
            _renderer.transform.localScale = size;

            Vector2 outlineSize = Vector2.one * _outlineThickness;
            _outline.transform.localScale = size + outlineSize;

            _renderer.material.SetTexture("_BaseMap", Picture);
            //renderer.material.mainTexture = picture;
        }
    }
}
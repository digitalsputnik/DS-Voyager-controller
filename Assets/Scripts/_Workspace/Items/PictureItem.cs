using UnityEngine;

namespace VoyagerController.Workspace
{
    public class PictureItem : WorkspaceItem
    {
        public int Order { get; set; } = 0;

        [Header("Renderer")]
        [SerializeField] private MeshRenderer _renderer = null;
        [SerializeField] private float _pixelsPerUnit = 20;

        [Header("Outline")]
        [SerializeField] private Transform _outline = null;
        [SerializeField] private float _outlineThickness = 0.2f;

        private Transform _transform;
        private Texture2D _picture;

        private static readonly int _baseMap = Shader.PropertyToID("_BaseMap");

        public override Vector3[] SelectPositions
        {
            get
            {
                var positions = new Vector3[5];
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
            _picture = data as Texture2D;
            _transform = transform;

            return _picture != null && base.Setup(data, uid);
        }

        private void Start()
        {
            if (!Metadata.Contains(Uid))
            {
                Metadata.Add<PictureData>(Uid);
                Metadata.Get<PictureData>(Uid).Texture = _picture;  
                PositionBasedOnCamera();
            }
            else
            {
                PositionBasedOnWorkspaceMapping();
            }
            
            SelectionMove.SelectionMoveEnded += SelectionMoveEnded;
        }

        private void OnDestroy()
        {
            if (Metadata.Contains(Uid))
            {
                Destroy(Metadata.Get<PictureData>(Uid).Texture);
                Metadata.Remove(Uid);   
            }
            
            SelectionMove.SelectionMoveEnded -= SelectionMoveEnded;
        }

        private void SelectionMoveEnded()
        {
            if (!WorkspaceSelection.Contains(this)) return;
            
            var mapping = Metadata.Get<PictureData>(Uid).WorkspaceMapping;
            var pos = _transform.position;
            mapping.Position = new[] { pos.x, pos.y };
            mapping.Rotation = _transform.eulerAngles.z;
            mapping.Scale = _transform.lossyScale.x;
        }

        protected override void Generate()
        {
            var size = new Vector2(_picture.width, _picture.height) / _pixelsPerUnit;
            var trans = _renderer.transform;
            var outlineSize = Vector2.one * _outlineThickness;
            
            size *= trans.localScale;
            trans.localScale = size;

            _outline.transform.localScale = size + outlineSize;
            _renderer.material.SetTexture(_baseMap, _picture);
        }
        
        public void PositionBasedOnWorkspaceMapping()
        {
            var mapping = Metadata.Get<PictureData>(Uid).WorkspaceMapping;
            var position = new Vector3(mapping.Position[0], mapping.Position[1], 0.0f);
            var rotation = new Vector3(0.0f, 0.0f, mapping.Rotation);
            var scale = Vector3.one * mapping.Scale;

            _transform = transform;
            _transform.localScale = scale;
            _transform.eulerAngles = rotation;
            _transform.position = position;
        }

        public void PositionBasedOnCamera()
        {
            SetupMeshSize();
            SetupOutlineSize();

            if (Camera.main != null)
            {
                var camPosition = Camera.main.transform.position;
                var pos = transform.position;
                
                pos.x = camPosition.x;
                pos.y = camPosition.y;
                
                _transform.position = pos;
            }
        }

        private void SetupMeshSize()
        {
            var maxScale = CalculateMeshMaxScale();
            var maxScaleAspect = maxScale.x / maxScale.y;
            var videoAspect = (float)_picture.width / _picture.height;

            var s = maxScale;

            if (videoAspect > maxScaleAspect)
                s.y = maxScale.y / videoAspect * maxScaleAspect;
            else if (videoAspect < maxScaleAspect)
                s.x = maxScale.x / maxScaleAspect * videoAspect;

            var diff = _renderer.transform.localScale.x / s.x;
            _transform.localScale = transform.localScale / diff;
        }

        private static Vector2 CalculateMeshMaxScale()
        {
            var screenWorldSize = VectorUtils.ScreenSizeWorldSpace;
            var width = screenWorldSize.x * 0.8f;
            var height = screenWorldSize.y - screenWorldSize.x * 0.2f;
            return new Vector2(width, height);
        }

        private void SetupOutlineSize()
        {
            var outlineSize = Vector2.one * _outlineThickness;
            Vector2 pictureSize = _renderer.transform.localScale;
            _outline.transform.localScale = pictureSize + outlineSize;
        }
    }
}
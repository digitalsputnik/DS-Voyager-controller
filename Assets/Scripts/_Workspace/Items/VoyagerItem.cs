using DigitalSputnik;
using DigitalSputnik.Voyager;
using UnityEngine;
using VoyagerController.Effects;

namespace VoyagerController.Workspace
{
    public class VoyagerItem : WorkspaceItem
    {
        public VoyagerLamp LampHandle { get; private set; }
        public int Order { get; set; } = 0;

        [Header("Pixels")]
        [SerializeField] private Transform _pixels = null;
        [SerializeField] private Vector2 _pixelSize = Vector2.one;
        [SerializeField] private MeshRenderer _renderer = null;
        [SerializeField] private Material _pixelsMaterial = null;
        
        [Header("Outline")]
        [SerializeField] private Transform _outline = null;
        [SerializeField] private float _outlineThickness = 0.2f;

        [Header("Info")]
        [SerializeField] private TextMesh _nameText = null;
        [SerializeField] private TextMesh _orderText = null;

        private LampMetadata _meta;
        private Texture2D _pixelsTexture;
        private static readonly int _baseMap = Shader.PropertyToID("_BaseMap");
        private LampConnectionType _connectionType;

        public override bool Setup(object data, string uid = "")
        {
            LampHandle = data as VoyagerLamp;

            if (LampHandle == null) return false;
            
            _meta = Metadata.Get(LampHandle.Serial);
            return base.Setup(data, uid);
        }

        private void Update()
        {
            UpdateConnectionType();
            UpdatePixels();
        }
        
        protected override void Generate()
        {
            SetupSizeAndPositions();
            SetupTextureAndMaterial();
            UpdateText();
        }

        private void SetupSizeAndPositions()
        {
            var size = new Vector2(LampHandle.PixelCount, 1) * _pixelSize;
            _pixels.localScale = size;

            var outlineSize = Vector2.one * _outlineThickness;
            _outline.localScale = size + outlineSize;

            _nameText.transform.position = new Vector2(0, _pixelSize.y * 0.75f);
            _orderText.transform.position = new Vector2(-_pixelSize.x * ((float)(LampHandle.PixelCount + 3) / 2), 0.0f);
        }

        private void SetupTextureAndMaterial()
        {
            if (_pixelsTexture == null)
            {
                Destroy(_pixelsTexture);
                _pixelsTexture = null;
            }
            
            _pixelsTexture = new Texture2D(LampHandle.PixelCount, 1);
            _pixelsTexture.filterMode = FilterMode.Point;
            _pixelsTexture.Apply();

            _renderer.material = _pixelsMaterial;
            _renderer.material.SetTexture(_baseMap, _pixelsTexture);
            
            // TODO: Implement DMX texture

            /*
            OLD DMX CODE
            if (!_lamp.dmxEnabled)
            {
                pixelsTexture = new Texture2D(lamp.length, 1);
                pixelsTexture.filterMode = FilterMode.Point;
                pixelsTexture.Apply();

                renderer.material = normMaterial;
                renderer.material.SetTexture("_BaseMap", pixelsTexture);
            }
            else
            {
                renderer.material = dmxMaterial;
            }
            */
        }

        private void UpdatePixels()
        {
            switch (_meta.Effect)
            {
                case VideoEffect video:
                {
                    var index = LampEffectsWorker.GetCurrentFrameOfVideo(LampHandle, video.Video);

                    if (_meta.FrameBuffer[index] != null)
                    {
                        var colors = _meta.FrameBuffer[index].ToColorArray();
                        var color = (Color)_meta.Itshe.ToColor();
                    
                        for (var i = 0; i < colors.Length; i++)
                            colors[i] = colors[i] * color;
                    
                        _pixelsTexture.SetPixels32(colors);
                        _pixelsTexture.Apply();
                    }

                    break;
                }
                case SyphonEffect _:
                {
                    var colors = _meta.PreviousStreamFrame.ToColorArray();
                    var color = (Color)_meta.Itshe.ToColor();
                    
                    for (var i = 0; i < colors.Length; i++)
                        colors[i] = colors[i] * color;
                
                    _pixelsTexture.SetPixels32(colors);
                    _pixelsTexture.Apply();
                    break;
                }
            }
        }

        private void UpdateConnectionType()
        {
            switch (LampHandle.Endpoint)
            {
                case LampNetworkEndPoint _:
                    _connectionType = LampConnectionType.NetConnection;
                    break;
                case BluetoothEndPoint _:
                    _connectionType = LampConnectionType.BluetoothConnection;
                    break;
            }
        }

        private void UpdateText()
        {
            _nameText.text = LampHandle.Serial;
        }

        public override Vector3[] SelectPositions
        {
            get
            {
                var positions = new Vector3[LampHandle.PixelCount];
                var pixels = PixelWorldPositions();

                for (var i = 0; i < LampHandle.PixelCount; i++)
                    positions[i] = new Vector3(pixels[i].x, pixels[i].y, 0.0f);

                return positions;
            }
        }

        public override Bounds Bounds => _renderer.bounds;

        private Vector2[] PixelWorldPositions()
        {
            var positions = new Vector2[LampHandle.PixelCount];
            var distance = 1.0f / LampHandle.PixelCount;
            var offset = distance / 2.0f;

            for (var i = 0; i < LampHandle.PixelCount; i++)
            {
                var x = i * distance - 0.5f + offset;
                var local = new Vector2(x, 0.0f);
                positions[i] = _pixels.TransformPoint(local);
            }

            return positions;
        }

        private enum LampConnectionType
        {
            Disconnected,
            NetConnection,
            BluetoothConnection
        }
    }
}
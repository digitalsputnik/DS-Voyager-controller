using System.Collections.Generic;
using DigitalSputnik;
using DigitalSputnik.Colors;
using DigitalSputnik.Voyager;
using UnityEngine;
using VoyagerController.Bluetooth;
using VoyagerController.Effects;
using VoyagerController.Mapping;

namespace VoyagerController.Workspace
{
    public class VoyagerItem : WorkspaceItem
    {
        public static bool ShowOrderNumber = false;

        public VoyagerLamp LampHandle { get; private set; }
        public int Order { get; set; }

        [Header("Pixels")]
        [SerializeField] private Transform _pixels = null;
        [SerializeField] private Vector2 _pixelSize = Vector2.one;
        [SerializeField] private MeshRenderer _renderer = null;
        [SerializeField] private Material _pixelsMaterial = null;
        
        [Header("Outline")]
        [SerializeField] private Transform _outline = null;
        [SerializeField] private MeshRenderer _outlineRenderer = null;
        [SerializeField] private float _outlineThickness = 0.2f;

        [Header("Info")]
        [SerializeField] private TextMesh _nameText = null;
        [SerializeField] private TextMesh _orderText = null;
        [SerializeField] private Color _normalTextColor = Color.black;
        [SerializeField] private Color _selectedTextColor = Color.black;

        [Header("DMX")]
        [SerializeField] private Material _dmxMaterial = null;

        private LampData _meta;
        private Texture2D _pixelsTexture;
        private Color _outlineColor;
        private static readonly int _baseMap = Shader.PropertyToID("_MainTex");
        private LampConnectionType _connectionType;
        private Transform _transform;
        private string _suffix;
        private string _prefix;
        private bool _dmxPrev = false;

        public override bool Setup(object data, string uid = "")
        {
            LampHandle = data as VoyagerLamp;

            if (LampHandle == null) return false;
            
            _meta = Metadata.Get<LampData>(LampHandle.Serial);

            _outlineColor = _outlineRenderer.material.color;

            return base.Setup(data, uid);
        }

        public string Suffix
        {
            get => _suffix;
            set
            {
                _suffix = value;
                UpdateText();
            }
        }

        public string Prefix
        {
            get => _prefix;
            set
            {
                _prefix = value;
                UpdateText();
            }
        }


        public bool Selected => WorkspaceSelection.Contains(this);

        public Vector2[] GetPixelWorldPositions()
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
        
        public void PositionLampBasedOnWorkspaceMapping()
        {
            var mapping = _meta.WorkspaceMapping;
            var position = new Vector3(mapping.Position[0], mapping.Position[1], 0.0f);
            var rotation = new Vector3(0.0f, 0.0f, mapping.Rotation);
            var scale = Vector3.one * mapping.Scale;

            _transform = transform;
            _transform.localScale = scale;
            _transform.eulerAngles = rotation;
            _transform.position = position;
        }

        public Vector3 GetWorkspacePosition()
        {
            var mapping = _meta.WorkspaceMapping;
            return new Vector3(mapping.Position[0], mapping.Position[1], 0.0f);
        }

        public Vector3 GetWorkspaceRotation()
        {
            var mapping = _meta.WorkspaceMapping;
            return new Vector3(0.0f, 0.0f, mapping.Rotation);
        }

        public Vector3 GetWorkspaceScale()
        {
            var mapping = _meta.WorkspaceMapping;
            return Vector3.one * mapping.Scale;
        }

        public Vector2 GetPixelSize() => _pixelSize;

        private void Start()
        {
            SaveWorkspaceMapping();
            SelectionMove.SelectionMoveEnded += SelectionMoved;
            WorkspaceSelection.SelectionChanged += SelectionChanged;
        }

        private void OnDestroy()
        {
            SelectionMove.SelectionMoveEnded -= SelectionMoved;
            WorkspaceSelection.SelectionChanged -= SelectionChanged;
        }

        private void Update()
        {
            UpdateConnectionType();
            if (CheckToUpdatePixels())
                UpdatePixels();
            UpdateText();
        }

        private bool CheckToUpdatePixels()
        {
            if (LampHandle.DmxModeEnabled)
            {
                if (!_dmxPrev)
                {
                    SetupDmxMaterial();
                    _dmxPrev = true;
                }
            }
            else
            {
                if (_dmxPrev)
                {
                    SetupPixelsMaterial();
                    _dmxPrev = false;
                }
            }

            return !LampHandle.DmxModeEnabled;
        }

        protected override void Generate()
        {
            SetupSizeAndPositions();
            
            if (LampHandle.DmxModeEnabled)
                SetupDmxMaterial();
            else
                SetupPixelsMaterial();
            
            UpdateText();
            PositionLampBasedOnWorkspaceMapping();
        }
        
        private void OrderNumberChanged(bool _) => UpdateText();

        private void SelectionMoved()
        {
            if (WorkspaceSelection.Contains(this))
            {
                if (!EffectMapper.EffectMappingIsActive)
                {
                    SaveWorkspaceMapping();
                }
                else
                {
                    _meta.EffectMapping = EffectMapper.CalculateLampEffectMapping(this);
                }
            }
        }

        private void SelectionChanged()
        {
            if (Selected)
            {
                _nameText.color = _selectedTextColor;
                _outlineRenderer.material.color = _selectedTextColor;
            }
            else
            {
                _nameText.color = _normalTextColor;
                _outlineRenderer.material.color = _outlineColor;
            }
        }

        private void SaveWorkspaceMapping()
        {
            var mapping = Metadata.Get<LampData>(LampHandle.Serial).WorkspaceMapping;
            var pos = _transform.position;
            mapping.Position = new[] { pos.x, pos.y };
            mapping.Rotation = _transform.eulerAngles.z;
            mapping.Scale = _transform.lossyScale.x;
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
        
        private void SetupPixelsMaterial()
        {
            if (_pixelsTexture != null)
            {
                Destroy(_pixelsTexture);
                _pixelsTexture = null;
            }
            
            _pixelsTexture = new Texture2D(LampHandle.PixelCount, 1);
            _pixelsTexture.filterMode = FilterMode.Point;
            _pixelsTexture.Apply();

            _renderer.material = _pixelsMaterial;
            _renderer.material.SetTexture(_baseMap, _pixelsTexture);
        }

        private void SetupDmxMaterial()
        {
            _renderer.material = _dmxMaterial;
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
                case SpoutEffect _:
                {
                    var colors = _meta.PreviousStreamFrame.ToColorArray();
                    var color = (Color)_meta.Itshe.ToColor();
                    
                    for (var i = 0; i < colors.Length; i++)
                        colors[i] = colors[i] * color;
                
                    _pixelsTexture.SetPixels32(colors);
                    _pixelsTexture.Apply();
                    break;
                }
                case ImageEffect _:
                    if (_meta.FrameBuffer[0] != null)
                    {
                        var colors = _meta.FrameBuffer[0].ToColorArray();
                        var color = (Color)_meta.Itshe.ToColor();
                    
                        for (var i = 0; i < colors.Length; i++)
                            colors[i] = colors[i] * color;
                    
                        _pixelsTexture.SetPixels32(colors);
                        _pixelsTexture.Apply();
                    }
                    break;
                default:
                    {
                        var rgb = ColorUtils.ItsheToRgb(_meta.Itshe);
                        var colors = ColorUtils.RgbToArray(rgb, LampHandle.PixelCount).ToColorArray();
                        _pixelsTexture.SetPixels32(colors);
                        _pixelsTexture.Apply();   
                    }
                    break;
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
            var info = new List<string>();

            if (ShowOrderNumber && Order > 0)
                info.Add(Order.ToString());
            
            info.Add(LampHandle.Serial);
            
            if (!LampHandle.Connected)
                info.Add(" Disconnected");
            
            if (!string.IsNullOrEmpty(Suffix))
                info.Add(Suffix);

            _nameText.text = string.Join(", ", info);

            if (LampHandle.Endpoint is BluetoothEndPoint)
                _nameText.text = "Bluetooth " + _nameText.text;

            _orderText.text = Prefix;
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
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using VoyagerController.Workspace;

namespace VoyagerController.UI
{
    public class AlignmentMenu : Menu
    {
        [Space(5)]
        [SerializeField] private GameObject _selectText = null;
        [SerializeField] private GameObject _selectDeselectBtn = null;
        [SerializeField] private GameObject _undoBtn = null;
        [SerializeField] private bool _workspaceAlignment = false;
        [SerializeField] private Transform _display = null;

        [Header("Alignment")]
        [SerializeField] private GameObject _alignTitle = null;
        [SerializeField] private GameObject _alignHorizontal = null;
        [SerializeField] private GameObject _alignVertical = null;
        [SerializeField] private GameObject _alignFlip = null;
        [SerializeField] private GameObject _alignScale = null;

        [Header("Distribute")]
        [SerializeField] private GameObject _distTitle = null;
        [SerializeField] private GameObject _distHorizontal = null;
        [SerializeField] private GameObject _distVertical = null;

        private readonly List<List<(VoyagerItem, WorkspaceMapping)>> _workspaceMappings = new List<List<(VoyagerItem, WorkspaceMapping)>>();
        private readonly List<List<(VoyagerItem, EffectMapping)>> _effectMappings = new List<List<(VoyagerItem, EffectMapping)>>();
        
        internal override void OnShow()
        {
            DisableEnableItems();
            WorkspaceSelection.SelectionChanged += DisableEnableItems;
        }

        internal override void OnHide()
        {
            WorkspaceSelection.SelectionChanged -= DisableEnableItems;
            _workspaceMappings.Clear();
        }

        private void DisableEnableItems()
        {
            var selectedLamps = WorkspaceSelection.GetSelected<VoyagerItem>().ToList();
            var lampsInWorkspace = WorkspaceManager.GetItems<VoyagerItem>().ToList();
            
            var has = lampsInWorkspace.Any();
            var one = selectedLamps.Any();
            var all = WorkspaceUtils.AllLampsSelected;

            _selectDeselectBtn.SetActive(has);
            _selectDeselectBtn.GetComponentInChildren<Text>().text = all ? "DESELECT ALL" : "SELECT ALL";
            
            _undoBtn.SetActive(_workspaceMappings.Count > 0);

            _selectText.SetActive(!one);

            _alignTitle.SetActive(one);
            _alignHorizontal.SetActive(one);
            _alignVertical.SetActive(one);
            _alignFlip.SetActive(one);
            _alignScale.SetActive(one);
             
            _distTitle.SetActive(one);
            _distHorizontal.SetActive(one);
            _distVertical.SetActive(one);
        }
        

        public void AlignHorizontally()
        {
            SaveMapping();
            AlignSelectedLampsHorizontally();
        }

        public void AlignVertically()
        {
            SaveMapping();
            AlignSelectedLampsVertically();
        }

        public void AlignFlip()
        {
            SaveMapping();
            FlipSelectedLamps();
        }

        public void AlignScale()
        {
            SaveMapping();
            ScaleSelectedLampsBasedOnBiggest();
        }

        public void DistributeHorizontally()
        {
            SaveMapping();
            DistributeSelectedLampsHorizontally();
        }

        public void DistributeVertically()
        {
            SaveMapping();
            DistributeSelectedLampsVertically();
        }

        public void SelectDeselect()
        {
            if (!WorkspaceUtils.AllLampsSelected)
                WorkspaceUtils.SelectAllLamps();
            else
                WorkspaceUtils.DeselectAllLamps();
        }

        public void Undo()
        {
            if (_workspaceAlignment)
                RecoverWorkspaceState();
            else
                RecoverEffectMapping();
        }
        
        public static Bounds SelectedLampsBounds()
        {
            var selected = WorkspaceSelection.GetSelected<VoyagerItem>().ToList();
            var bounds = new Bounds(selected.First().SelectPositions[0], Vector3.zero);
            foreach (var position in selected.SelectMany(lampItem => lampItem.SelectPositions))
                bounds.Encapsulate(position);
            return bounds;
        }

        private void AlignSelectedLampsHorizontally()
        {
            var rotations = new List<float> { 0.0f, 180.0f, 360.0f };
            AlignSelectedLampsToRotations(rotations);
        }

        private void AlignSelectedLampsVertically()
        {
            var rotations = new List<float> { 90.0f, 270.0f };
            AlignSelectedLampsToRotations(rotations);
        }

        private void AlignSelectedLampsToRotations(IReadOnlyCollection<float> rotations)
        {
            foreach (var item in WorkspaceSelection.GetSelected<VoyagerItem>().ToList())
            {
                WorkspaceSelection.DeselectItem(item);
                
                var position = GetPosition(item);
                var scale = GetScale(item).x;
                var rotation = rotations.OrderBy(x => math.abs(GetRotation(item).z - x)).First();

                var meta = Metadata.Get<LampData>(item.LampHandle.Serial);
                
                var mapping = new WorkspaceMapping
                {
                    Position = new[] { position.x, position.y },
                    Rotation = rotation,
                    Scale = scale
                };

                if (_workspaceAlignment)
                {

                    meta.WorkspaceMapping = mapping;
                    item.PositionBasedOnWorkspaceMapping();   
                }
                else
                {
                    var prev = meta.WorkspaceMapping;

                    meta.WorkspaceMapping = mapping;
                    item.PositionBasedOnWorkspaceMapping();
                    meta.WorkspaceMapping = prev;
                    meta.EffectMapping = CalculateLampEffectMapping(item, _display);
                }

                WorkspaceSelection.SelectItem(item);
            }

            SelectionMove.RaiseMovedEvent();
        }

        private void FlipSelectedLamps()
        {
            foreach (var item in WorkspaceSelection.GetSelected<VoyagerItem>().ToList())
            {
                WorkspaceSelection.DeselectItem(item);
                
                var position = GetPosition(item);
                var rotation = GetRotation(item).z + 180.0f;
                var scale = GetScale(item).x;
                
                var meta = Metadata.Get<LampData>(item.LampHandle.Serial);
                
                var mapping = new WorkspaceMapping
                {
                    Position = new[] { position.x, position.y },
                    Rotation = rotation,
                    Scale = scale
                };

                if (_workspaceAlignment)
                {

                    meta.WorkspaceMapping = mapping;
                    item.PositionBasedOnWorkspaceMapping();   
                }
                else
                {
                    var prev = meta.WorkspaceMapping;

                    meta.WorkspaceMapping = mapping;
                    item.PositionBasedOnWorkspaceMapping();
                    meta.WorkspaceMapping = prev;
                    meta.EffectMapping = CalculateLampEffectMapping(item, _display);
                }

                WorkspaceSelection.SelectItem(item);
            }

            SelectionMove.RaiseMovedEvent();
        }

        private void ScaleSelectedLampsBasedOnBiggest()
        {
            var longest = WorkspaceSelection.GetSelected<VoyagerItem>()
                .OrderByDescending(l => l.GetComponentInChildren<MeshRenderer>().transform.lossyScale.x)
                .FirstOrDefault();

            if (longest != null)
            {
                float shortScale;
                float longScale;

                if (longest.LampHandle.PixelCount > 50)
                {
                    shortScale = GetScale(longest).x * (83.0f / 42.0f);
                    longScale = GetScale(longest).x;
                }
                else
                {
                    shortScale = GetScale(longest).x;
                    longScale = GetScale(longest).x * (42.0f / 83.0f);
                }

                foreach (var item in WorkspaceSelection.GetSelected<VoyagerItem>().ToList())
                {
                    WorkspaceSelection.DeselectItem(item); 
                    
                    var meta = Metadata.Get<LampData>(item.LampHandle.Serial);
                    var prev = meta.WorkspaceMapping;
                    var position = GetPosition(item);
                    var rotation = GetRotation(item).z;

                    if (item.LampHandle.PixelCount > 50)
                    {
                        var mapping = new WorkspaceMapping
                        {
                            Position = new[] { position.x, position.y },
                            Rotation = rotation,
                            Scale = longScale
                        };
                        
                        meta.WorkspaceMapping = mapping;
                    }
                    else
                    {
                        var mapping = new WorkspaceMapping
                        {
                            Position = new[] { position.x, position.y },
                            Rotation = rotation,
                            Scale = shortScale
                        };
                        
                        meta.WorkspaceMapping = mapping;
                    }
                    
                    item.PositionBasedOnWorkspaceMapping();
                    
                    if (!_workspaceAlignment)
                    {
                        meta.WorkspaceMapping = prev;
                        meta.EffectMapping = CalculateLampEffectMapping(item, _display);
                    }
                    
                    WorkspaceSelection.SelectItem(item);
                }
            }

            SelectionMove.RaiseMovedEvent();
        }

        private void DistributeSelectedLampsHorizontally()
        {
            var lamps = WorkspaceSelection.GetSelected<VoyagerItem>()
                .OrderBy(l => GetPosition(l).x)
                .ToList();

            var positions = SelectedHorizontalAlignment();

            WorkspaceSelection.Clear();

            for (var i = 0; i < lamps.Count; i++)
            {
                var item = lamps[i];
                var position = positions[i];
                var rotation = GetRotation(item).z;
                var scale = GetScale(item).x;

                var meta = Metadata.Get<LampData>(item.LampHandle.Serial);
                
                var mapping = new WorkspaceMapping
                {
                    Position = new[] { position.x, position.y },
                    Rotation = rotation,
                    Scale = scale
                };

                if (_workspaceAlignment)
                {

                    meta.WorkspaceMapping = mapping;
                    item.PositionBasedOnWorkspaceMapping();   
                }
                else
                {
                    var prev = meta.WorkspaceMapping;

                    meta.WorkspaceMapping = mapping;
                    item.PositionBasedOnWorkspaceMapping();
                    meta.WorkspaceMapping = prev;
                    meta.EffectMapping = CalculateLampEffectMapping(item, _display);
                }
                
                WorkspaceSelection.SelectItem(item);
            }

            SelectionMove.RaiseMovedEvent();
        }

        private Vector2[] SelectedHorizontalAlignment()
        {
            var selectedLamps = WorkspaceSelection.GetSelected<VoyagerItem>().ToList();
            var bounds = SelectedLampsBounds();
            var count = selectedLamps.Count;
            var points = new List<Vector2>();

            var start = selectedLamps.Min(l => GetPosition(l).x);
            var max = selectedLamps.Max(l => GetPosition(l).x);
            var step = count > 1 ? (max - start) / (count - 1) : 0;
            var y = bounds.center.y;

            for (var i = 0; i < count; i++)
                points.Add(new float2(start + step * i, y));

            return points.ToArray();
        }

        private void DistributeSelectedLampsVertically()
        {
            var lamps = WorkspaceSelection.GetSelected<VoyagerItem>()
                .OrderBy(l => GetPosition(l).y)
                .ToList();

            var positions = SelectedVerticalAlignment();

            WorkspaceSelection.Clear();

            for (var i = 0; i < lamps.Count; i++)
            {
                var item = lamps[i];
                var position = positions[i];
                var rotation = GetRotation(item).z;
                var scale = GetScale(item).x;
                
                var meta = Metadata.Get<LampData>(item.LampHandle.Serial);
                
                var mapping = new WorkspaceMapping
                {
                    Position = new[] { position.x, position.y },
                    Rotation = rotation,
                    Scale = scale
                };

                if (_workspaceAlignment)
                {

                    meta.WorkspaceMapping = mapping;
                    item.PositionBasedOnWorkspaceMapping();   
                }
                else
                {
                    var prev = meta.WorkspaceMapping;

                    meta.WorkspaceMapping = mapping;
                    item.PositionBasedOnWorkspaceMapping();
                    meta.WorkspaceMapping = prev;
                    meta.EffectMapping = CalculateLampEffectMapping(item, _display);
                }
                
                WorkspaceSelection.SelectItem(item);
            }

            SelectionMove.RaiseMovedEvent();
        }

        private Vector2[] SelectedVerticalAlignment()
        {
            var selected = WorkspaceSelection.GetSelected<VoyagerItem>().ToList();
            var bounds = SelectedLampsBounds();
            var count = selected.Count;
            var points = new List<Vector2>();

            var start = selected.Min(l => GetPosition(l).y);
            var max = selected.Max(l => GetPosition(l).y);
            var step = count > 1 ? (max - start) / (count - 1) : 0;
            var x = bounds.center.x;

            for (var i = 0; i < count; i++)
                points.Add(new float2(x, start + step * i));

            return points.ToArray();
        }

        private void SaveMapping()
        {
            if (_workspaceAlignment)
                SaveWorkspaceMapping();
            else
                SaveEffectMapping();
        }

        private void SaveWorkspaceMapping()
        {
            var items = new List<(VoyagerItem, WorkspaceMapping)>();

            foreach (var item in WorkspaceManager.GetItems<VoyagerItem>())
            {
                var mapping = Metadata.Get<LampData>(item.LampHandle.Serial).WorkspaceMapping;
                items.Add((item, mapping));
            }

            if (_workspaceMappings.Count == 10)
                _workspaceMappings.RemoveAt(9);
            
            _workspaceMappings.Insert(0, items);
        }

        private void RecoverWorkspaceState()
        {
            WorkspaceSelection.Clear();
            
            var workspace = WorkspaceManager.GetItems<VoyagerItem>().ToList();
            
            foreach (var (item, mapping) in _workspaceMappings[0])
            {
                if (workspace.All(i => i.LampHandle != item.LampHandle)) continue;
                
                Metadata.Get<LampData>(item.LampHandle.Serial).WorkspaceMapping = mapping;
                item.PositionBasedOnWorkspaceMapping();
            }

            foreach (var voyager in workspace)
                WorkspaceSelection.SelectItem(voyager);

            _workspaceMappings.RemoveAt(0);
            DisableEnableItems();
        }

        private void SaveEffectMapping()
        {
            var items = new List<(VoyagerItem, EffectMapping)>();
            
            foreach (var item in WorkspaceManager.GetItems<VoyagerItem>())
            {
                var mapping = Metadata.Get<LampData>(item.LampHandle.Serial).EffectMapping;
                items.Add((item, mapping));
            }
            
            if (_effectMappings.Count == 10)
                _effectMappings.RemoveAt(9);
            
            _effectMappings.Insert(0, items);
        }

        private void RecoverEffectMapping()
        {
            WorkspaceSelection.Clear();
            
            var workspace = WorkspaceManager.GetItems<VoyagerItem>().ToList();
            
            foreach (var (item, mapping) in _effectMappings[0])
            {
                if (workspace.All(i => i.LampHandle != item.LampHandle)) continue;
                
                Metadata.Get<LampData>(item.LampHandle.Serial).EffectMapping = mapping;
                item.PositionBasedOnEffectMapping(_display);
            }

            foreach (var voyager in workspace)
                WorkspaceSelection.SelectItem(voyager);

            _effectMappings.RemoveAt(0);
            DisableEnableItems();
        }

        private Vector3 GetPosition(VoyagerItem voyager)
        {
            return _workspaceAlignment ? voyager.GetWorkspacePosition() : voyager.GetEffectMappingPosition(_display);
        }
        
        private Vector3 GetRotation(VoyagerItem voyager)
        {
            return _workspaceAlignment ? voyager.GetWorkspaceRotation() : voyager.GetEffectMappingRotation(_display);
        }
        
        private Vector3 GetScale(VoyagerItem voyager)
        {
            return _workspaceAlignment ? voyager.GetWorkspaceScale() : voyager.GetEffectMappingScale(_display);
        }
        
        public static EffectMapping CalculateLampEffectMapping(VoyagerItem voyager, Transform display)
        {
            var pixelPositions = voyager.GetPixelWorldPositions();
            var pixels = new [] { pixelPositions.First(), pixelPositions.Last() };

            for (var i = 0; i < 2; i++)
            {
                var pixel = pixels[i];
                var local = display.InverseTransformPoint(pixel);

                var x = local.x + 0.5f;
                var y = local.y + 0.5f;
                
                pixels[i] = new Vector2(x, y);
            }

            return new EffectMapping
            {
                X1 = pixels[0].x,
                X2 = pixels[1].x,
                Y1 = pixels[0].y,
                Y2 = pixels[1].y
            };
        }
    }
}
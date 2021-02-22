using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using VoyagerController.Workspace;

namespace VoyagerController.UI
{
    public class AlignmentMenu : Menu
    {
        [SerializeField] private GameObject _selectText = null;
        [SerializeField] private GameObject _selectDeselectBtn = null;
        [SerializeField] private GameObject _undoBtn = null;

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
        
        internal override void OnShow()
        {
            DisableEnableItems();
            WorkspaceSelection.SelectionChanged += DisableEnableItems;
        }

        internal override void OnHide()
        {
            WorkspaceSelection.SelectionChanged -= DisableEnableItems;
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
            
            // _undoBtn.SetActive(_states.Count > 0);

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
            AlignSelectedLampsHorizontally();
        }

        public void AlignVertically()
        {
            AlignSelectedLampsVertically();
        }

        public void AlignFlip()
        {
            FlipSelectedLamps();
        }

        public void AlignScale()
        {
            ScaleSelectedLampsBasedOnBiggest();
        }

        public void DistributeHorizontally()
        {
            DistributeSelectedLampsHorizontally();
        }

        public void DistributeVertically()
        {
            DistributeSelectedLampsVertically();
        }

        public void SelectDeselect()
        {
            if (!WorkspaceUtils.AllLampsSelected)
                WorkspaceUtils.SelectAllLamps();
            else
                WorkspaceUtils.DeselectAllLamps();
        }
        
        public static Bounds SelectedLampsBounds()
        {
            var selected = WorkspaceSelection.GetSelected<VoyagerItem>().ToList();
            var bounds = new Bounds(selected.First().SelectPositions[0], Vector3.zero);
            foreach (var position in selected.SelectMany(lampItem => lampItem.SelectPositions))
                bounds.Encapsulate(position);
            return bounds;
        }

        private static void AlignSelectedLampsHorizontally()
        {
            var rotations = new List<float> { 0.0f, 180.0f, 360.0f };
            AlignSelectedLampsToRotations(rotations);
        }

        private static void AlignSelectedLampsVertically()
        {
            var rotations = new List<float> { 90.0f, 270.0f };
            AlignSelectedLampsToRotations(rotations);
        }

        private static void AlignSelectedLampsToRotations(IReadOnlyCollection<float> rotations)
        {
            foreach (var item in WorkspaceSelection.GetSelected<VoyagerItem>().ToList())
            {
                var position = item.GetWorkspacePosition();
                var scale = item.GetWorkspaceScale().x;
                var rotation = rotations.OrderBy(x => math.abs(item.GetWorkspaceRotation().z - x)).First();
                
                var mapping = new WorkspaceMapping
                {
                    Position = new[] { position.x, position.y },
                    Rotation = rotation,
                    Scale = scale
                };

                WorkspaceSelection.DeselectItem(item);
                Metadata.GetLamp(item.LampHandle.Serial).WorkspaceMapping = mapping;
                item.PositionLampBasedOnWorkspaceMapping();
                WorkspaceSelection.SelectItem(item);
            }

            SelectionMove.RaiseMovedEvent();
        }

        public static void FlipSelectedLamps()
        {
            foreach (var item in WorkspaceSelection.GetSelected<VoyagerItem>().ToList())
            {
                var position = item.GetWorkspacePosition();
                var rotation = item.GetWorkspaceRotation().z + 180.0f;
                var scale = item.GetWorkspaceScale().x;
             
                var mapping = new WorkspaceMapping
                {
                    Position = new[] { position.x, position.y },
                    Rotation = rotation,
                    Scale = scale
                };

                WorkspaceSelection.DeselectItem(item);
                Metadata.GetLamp(item.LampHandle.Serial).WorkspaceMapping = mapping;
                item.PositionLampBasedOnWorkspaceMapping();
                WorkspaceSelection.SelectItem(item);
            }

            SelectionMove.RaiseMovedEvent();
        }

        public static void ScaleSelectedLampsBasedOnBiggest()
        {
            var longest = WorkspaceSelection.GetSelected<VoyagerItem>()
                .OrderByDescending(l => l.GetComponentInChildren<MeshRenderer>().transform.lossyScale.x)
                .FirstOrDefault();

            if (longest != null)
            {
                var scale = longest.GetWorkspaceScale().x / longest.LampHandle.PixelCount;

                float shortScale;
                float longScale;

                if (longest.LampHandle.PixelCount > 50)
                {
                    shortScale = longest.GetWorkspaceScale().x * (83.0f / 42.0f);
                    longScale = longest.GetWorkspaceScale().x;
                }
                else
                {
                    shortScale = longest.GetWorkspaceScale().x;
                    longScale = longest.GetWorkspaceScale().x * (42.0f / 83.0f);
                }

                foreach (var item in WorkspaceSelection.GetSelected<VoyagerItem>().ToList())
                {
                    var position = item.GetWorkspacePosition();
                    var rotation = item.GetWorkspaceRotation().z;
                    
                    WorkspaceSelection.DeselectItem(item);

                    if (item.LampHandle.PixelCount > 50)
                    {
                        var mapping = new WorkspaceMapping
                        {
                            Position = new[] { position.x, position.y },
                            Rotation = rotation,
                            Scale = longScale
                        };
                        
                        Metadata.GetLamp(item.LampHandle.Serial).WorkspaceMapping = mapping;
                    }
                    else
                    {
                        var mapping = new WorkspaceMapping
                        {
                            Position = new[] { position.x, position.y },
                            Rotation = rotation,
                            Scale = shortScale
                        };
                        
                        Metadata.GetLamp(item.LampHandle.Serial).WorkspaceMapping = mapping;
                    }
                    
                    item.PositionLampBasedOnWorkspaceMapping();
                    WorkspaceSelection.SelectItem(item);
                }
            }

            SelectionMove.RaiseMovedEvent();
        }

        public static void DistributeSelectedLampsHorizontally()
        {
            var lamps = WorkspaceSelection.GetSelected<VoyagerItem>()
                .OrderBy(l => l.GetWorkspacePosition().x)
                .ToList();

            var positions = SelectedHorizontalAlignment();

            WorkspaceSelection.Clear();

            for (var i = 0; i < lamps.Count; i++)
            {
                var item = lamps[i];
                var position = positions[i];
                var rotation = item.GetWorkspaceRotation().z;
                var scale = item.GetWorkspaceScale().x;
             
                var mapping = new WorkspaceMapping
                {
                    Position = new[] { position.x, position.y },
                    Rotation = rotation,
                    Scale = scale
                };

                Metadata.GetLamp(item.LampHandle.Serial).WorkspaceMapping = mapping;
                item.PositionLampBasedOnWorkspaceMapping();
                WorkspaceSelection.SelectItem(item);
            }

            SelectionMove.RaiseMovedEvent();
        }

        private static Vector2[] SelectedHorizontalAlignment()
        {
            var selectedLamps = WorkspaceSelection.GetSelected<VoyagerItem>().ToList();
            var bounds = SelectedLampsBounds();
            var count = selectedLamps.Count;
            var points = new List<Vector2>();

            var start = selectedLamps.Min(l => l.GetWorkspacePosition().x);
            var max = selectedLamps.Max(l => l.GetWorkspacePosition().x);
            var step = count > 1 ? (max - start) / (count - 1) : 0;
            var y = bounds.center.y;

            for (var i = 0; i < count; i++)
                points.Add(new float2(start + step * i, y));

            return points.ToArray();
        }

        public static void DistributeSelectedLampsVertically()
        {
            var lamps = WorkspaceSelection.GetSelected<VoyagerItem>()
                .OrderBy(l => l.GetWorkspacePosition().y)
                .ToList();

            var positions = SelectedVerticalAlignment();

            WorkspaceSelection.Clear();

            for (var i = 0; i < lamps.Count; i++)
            {
                var item = lamps[i];
                var position = positions[i];
                var rotation = item.GetWorkspaceRotation().z;
                var scale = item.GetWorkspaceScale().x;
             
                var mapping = new WorkspaceMapping
                {
                    Position = new[] { position.x, position.y },
                    Rotation = rotation,
                    Scale = scale
                };

                Metadata.GetLamp(item.LampHandle.Serial).WorkspaceMapping = mapping;
                item.PositionLampBasedOnWorkspaceMapping();
                WorkspaceSelection.SelectItem(item);
            }

            SelectionMove.RaiseMovedEvent();
        }

        private static Vector2[] SelectedVerticalAlignment()
        {
            var selected = WorkspaceSelection.GetSelected<VoyagerItem>().ToList();
            var bounds = SelectedLampsBounds();
            var count = selected.Count;
            var points = new List<Vector2>();

            var start = selected.Min(l => l.GetWorkspacePosition().y);
            var max = selected.Max(l => l.GetWorkspacePosition().y);
            var step = count > 1 ? (max - start) / (count - 1) : 0;
            var x = bounds.center.x;

            for (var i = 0; i < count; i++)
                points.Add(new float2(x, start + step * i));

            return points.ToArray();
        }
    }
}
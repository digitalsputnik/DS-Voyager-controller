using System.Linq;
using UnityEngine;
using VoyagerController.Effects;
using VoyagerController.UI;
using VoyagerController.Workspace;

namespace VoyagerController.Mapping
{
    public class EffectMapper : MonoBehaviour
    {
        #region Singleton
        private static EffectMapper _instance;
        private void Awake() => _instance = this;
        #endregion

        private const float ANIMATION_TIME = 0.1f;
        
        public static bool EffectMappingIsActive = false;

        [SerializeField] private Transform _displayTransform = null;
        [SerializeField] private MenuContainer _menuContainer = null;
        [SerializeField] private EffectMappingMenu _mappingMenu = null;
        [SerializeField] private Menu _exitMenu = null;
        
        private EffectDisplay[] _displays;
        private EffectDisplay _activeDisplay;
        private Effect _effect;

        private void Start()
        {
            _displays = _displayTransform.GetComponents<EffectDisplay>() ?? new EffectDisplay[0];
            gameObject.SetActive(false);
        }

        public static void EnterEffectMapping(Effect effect)
        {
            _instance.CleanPreviousDisplay();
            _instance.gameObject.SetActive(true);
            _instance._effect = effect;
            _instance._menuContainer.ShowMenu(_instance._mappingMenu);

            var selected = WorkspaceSelection.GetSelected<VoyagerItem>().ToArray();

            foreach (var voyager in WorkspaceManager.GetItems<VoyagerItem>().ToArray())
            {
                if (selected.Contains(voyager))
                {
                    var meta = Metadata.Get(voyager.LampHandle.Serial);
                    var point1 = new Vector3(meta.EffectMapping.X1 - 0.5f, meta.EffectMapping.Y1 - 0.5f);
                    var point2 = new Vector3(meta.EffectMapping.X2 - 0.5f, meta.EffectMapping.Y2 - 0.5f);
                    var transform = voyager.transform;

                    point1 = _instance._displayTransform.TransformPoint(point1);
                    point2 = _instance._displayTransform.TransformPoint(point2);
                    
                    var center = (point1 + point2) / 2.0f;
                    var angle = AngleFromTo(point1, point2);

                    var position = new Vector3(center.x, center.y, 0.0f);
                    var rotation = new Vector3(0.0f, 0.0f, angle);

                    var distance = Vector3.Distance(point1, point2);
                    var scale = Vector3.one * distance / ((voyager.LampHandle.PixelCount - 1) * voyager.GetPixelSize().x);

                    LeanTween.move(transform.gameObject, position, ANIMATION_TIME);
                    LeanTween.rotate(transform.gameObject, rotation, ANIMATION_TIME);
                    LeanTween.scale(transform.gameObject, scale, ANIMATION_TIME);
                }
                else
                {
                    voyager.gameObject.SetActive(false);
                }
            }

            WorkspaceSelection.Clear();

            switch (effect)
            {
                case VideoEffect video:
                    _instance.PrepareDisplay<VideoEffectDisplay>(video);
                    break;
                case SyphonEffect syphon:
                    _instance.PrepareDisplay<SyphonEffectDisplay>(syphon);
                    break;
            }

            SelectionMove.SelectionMoveEnded += SelectedItemsMoved;
            EffectMappingIsActive = true;
        }

        private static void SelectedItemsMoved()
        {
            foreach (var voyager in WorkspaceSelection.GetSelected<VoyagerItem>())
            {
                var mapping = CalculateLampEffectMapping(voyager);
                var meta = Metadata.Get(voyager.LampHandle.Serial);
                
                meta.EffectMapping = mapping;
                LampEffectsWorker.ApplyEffectToLamp(voyager.LampHandle, _instance._effect);
            }
        }

        public static void LeaveEffectMapping()
        {
            _instance.CleanPreviousDisplay();
            _instance.gameObject.SetActive(false);
            _instance._menuContainer.ShowMenu(_instance._exitMenu);
            
            WorkspaceSelection.Clear();

            foreach (var voyager in WorkspaceManager.GetItems<VoyagerItem>())
            {
                voyager.gameObject.SetActive(true);

                var position = voyager.GetWorkspacePosition();
                var rotation = voyager.GetWorkspaceRotation();
                var scale = voyager.GetWorkspaceScale();
                
                LeanTween.move(voyager.gameObject, position, ANIMATION_TIME);
                LeanTween.rotate(voyager.gameObject, rotation, ANIMATION_TIME);
                LeanTween.scale(voyager.gameObject, scale, ANIMATION_TIME);
            }
            
            EffectMappingIsActive = false;
        }
        
        public static EffectMapping CalculateLampEffectMapping(VoyagerItem voyager)
        {
            var pixelPositions = voyager.GetPixelWorldPositions();
            var pixels = new [] { pixelPositions.First(), pixelPositions.Last() };

            for (var i = 0; i < 2; i++)
            {
                var pixel = pixels[i];
                var local = _instance._displayTransform.InverseTransformPoint(pixel);

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
        
        private static float AngleFromTo(Vector2 from, Vector2 to)
        {
            var direction = to - from;
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (angle < 0f) angle += 360f;
            return angle;
        }
        
        private void CleanPreviousDisplay()
        {
            if (_activeDisplay != null)
            {
                _activeDisplay.Clean();
                _activeDisplay = null;
            }
        }

        private void PrepareDisplay<T>(Effect effect) where T : EffectDisplay
        {
            var display = _displays.OfType<T>().First();
            display.Setup(effect);
            _activeDisplay = display;
        }
    }
}

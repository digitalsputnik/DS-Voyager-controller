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
            _instance.MoveLampsToCorrectPosition();
            _instance._menuContainer.ShowMenu(_instance._mappingMenu);
            
            switch (effect)
            {
                case VideoEffect video:
                    _instance.PrepareDisplay<VideoEffectDisplay>(video);
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
                voyager.PositionLampBasedOnWorkspaceMapping();
            
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

        private void MoveLampsToCorrectPosition()
        {
            
        }

        private void CleanPreviousDisplay()
        {
            if (_activeDisplay != null)
            {
                _activeDisplay.Clean();
                _activeDisplay = null;
            }
        }

        private void PrepareDisplay<T>(Effect effect) where T : VideoEffectDisplay
        {
            var display = _displays.OfType<T>().First();
            display.Setup(effect);
            _activeDisplay = display;
        }
    }
}

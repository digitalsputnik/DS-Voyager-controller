using System.Linq;
using DigitalSputnik.Colors;
using UnityEngine;
using UnityEngine.UI;
using VoyagerController.Workspace;

namespace VoyagerController.UI
{
    [RequireComponent(typeof(Button))]
    public class ColorWheelButton : MonoBehaviour
    {
        [SerializeField] private Image _previewColor = null;

        private Itshe _currentItshe;
        private Button _button;

        private void Start()
        {
            _button = GetComponent<Button>();
            WorkspaceSelection.SelectionChanged += SelectionChanged;
            _button.onClick.AddListener(Click);
            SelectionChanged();
        }

        private void OnDestroy()
        {
            WorkspaceSelection.SelectionChanged -= SelectionChanged;
        }

        private void SelectionChanged()
        {
            var lamp = WorkspaceSelection.GetSelected<VoyagerItem>().FirstOrDefault();
            
            if (lamp != null)
            {
                var meta = Metadata.Get<LampData>(lamp.LampHandle.Serial);

                _currentItshe = meta.Itshe;
                _previewColor.color = _currentItshe.ToColor();
                _previewColor.gameObject.SetActive(true);
                _button.interactable = true;
            }
            else
            {
                _previewColor.gameObject.SetActive(false);
                _button.interactable = false;
            }
        }

        public void Click()
        {
            /*
            DialogBox.PauseDialogues();
            ColorwheelManager.instance.OpenColorwheel(_currentItshe, ItsheChanged);
            */
            
            ColorWheelManager.OpenColorWheel(_currentItshe, ItsheChanged);
        }

        private void ItsheChanged(Itshe itshe)
        {
            _currentItshe = itshe;
            _previewColor.color = itshe.ToColor();
            foreach (var item in WorkspaceSelection.GetSelected<VoyagerItem>().ToList())
                LampEffectsWorker.ApplyItsheToVoyager(item.LampHandle, itshe);
        }
    }
}
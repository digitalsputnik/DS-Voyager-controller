using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VoyagerController.Workspace;

namespace VoyagerController.UI
{
    public class IdentifyLamps : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Color _pressedColor = Color.white;
        [SerializeField] private Color _releasedColor = Color.white;
        [SerializeField] private Image _image = null;
        [SerializeField] private float _duration = 0.3f;

        private bool _send;
        private float _timestamp;

        private void Start()
        {
            WorkspaceSelection.SelectionChanged += OnSelectionChanged;
            OnPointerUp(null);
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_send && Time.time - _timestamp > 0.16f)
            {
                foreach (var lamp in WorkspaceSelection.GetSelected<VoyagerItem>())
                    lamp.LampHandle.OverridePixels(ApplicationSettings.IdentificationColor, _duration);
                _timestamp = Time.time;
            }
        }

        private void OnDestroy()
        {
            WorkspaceSelection.SelectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged()
        {
            gameObject.SetActive(WorkspaceSelection.GetSelected<VoyagerItem>().Any());
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _image.color = _pressedColor;
            _send = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _image.color = _releasedColor;
            _send = false;
        }
    }
}
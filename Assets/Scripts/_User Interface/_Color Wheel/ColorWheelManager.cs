using DigitalSputnik.Colors;
using UnityEngine;

namespace VoyagerController.UI
{
    public class ColorWheelManager : MonoBehaviour
    {
        private static ColorWheelManager _instance;
        private void Awake() => _instance = this;

        [SerializeField] private InspectorMenuContainer _container = null;
        [SerializeField] private ColorWheelMenu _colorWheelMenu = null;

        private ColorWheelHandler _picked;

        public static void OpenColorWheel(Itshe itshe, ColorWheelHandler picked)
        {
            _instance._picked = picked;
            _instance._colorWheelMenu.SetItsh(itshe);
            _instance._container.ShowMenu(_instance._colorWheelMenu);
        }

        public static void ValuePicked(Itshe itshe)
        {
            _instance._picked?.Invoke(itshe);
        }
    }

    public delegate void ColorWheelHandler(Itshe itshe);
}
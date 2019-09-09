using UnityEngine;
using VoyagerApp.UI.Menus;

namespace VoyagerApp.UI
{
    public class ColorwheelManager : MonoBehaviour
    {
        public static ColorwheelManager instance;
        void Awake() => instance = this;

        [SerializeField] InspectorMenuContainer container   = null;
        [SerializeField] ColorWheelMenu colorwheelMenu      = null;

        ColorwheelHandler onPick;

        public void OpenColorwheel(Itshe itshe, ColorwheelHandler onPick)
        {
            this.onPick = onPick;
            colorwheelMenu.SetItsh(itshe);
            container.ShowMenu(colorwheelMenu);
        }

        public void ValuePicked(Itshe itshe)
        {
            onPick?.Invoke(itshe);
        }
    }

    public delegate void ColorwheelHandler(Itshe itshe);
}
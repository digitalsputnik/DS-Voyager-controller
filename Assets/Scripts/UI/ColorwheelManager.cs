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

        public void OpenColorwheel(Itsh itsh, ColorwheelHandler onPick)
        {
            this.onPick = onPick;
            colorwheelMenu.SetItsh(itsh);
            container.ShowMenu(colorwheelMenu);
        }

        public void ValuePicked(Itsh itsh)
        {
            onPick?.Invoke(itsh);
        }
    }

    public delegate void ColorwheelHandler(Itsh itsh);
}
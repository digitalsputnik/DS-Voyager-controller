using DigitalSputnik.Colors;
using UnityEngine;

namespace VoyagerController
{
    public static class VectorUtils
    {
        public static Vector2 ScreenSizeWorldSpace
        {
            get
            {
                var min = new Vector2(0, 0);
                var max = new Vector2(Screen.width, Screen.height);

                Camera cam = Camera.main;

                var wMin = cam.ScreenToWorldPoint(min);
                var wMax = cam.ScreenToWorldPoint(max);

                float width = Mathf.Abs(wMax.x - wMin.x);
                float height = Mathf.Abs(wMax.y - wMin.y);

                return new Vector2(width, height);
            }
        }
    }
}
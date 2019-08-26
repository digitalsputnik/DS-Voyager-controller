using UnityEngine;

namespace VoyagerApp.Utilities
{
    public static class VectorUtils
    {
        public static Vector2 ScreenMiddlePosition
        {
            get
            {
                Camera cam = Camera.main;
                var point = new Vector2(Screen.width / 2f, Screen.height / 2f);
                return cam.ScreenToWorldPoint(point);
            }
        }

        public static Vector2 ScreenRandomVerticalPosition
        {
            get
            {
                Camera cam = Camera.main;
                var upper = new Vector2(Screen.width / 2f, 0);
                var bottom = new Vector2(Screen.width / 2f, Screen.height / 2f);
                float t = Random.value;
                Vector2 point = Vector2.Lerp(upper, bottom, t);
                return cam.ScreenToWorldPoint(point);
            }
        }
    }
}
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
                var upper = new Vector2(Screen.width / 2f, Screen.height);
                var bottom = new Vector2(Screen.width / 2f, 0);
                float t = Random.value;
                Vector2 point = Vector2.Lerp(upper, bottom, t);
                return cam.ScreenToWorldPoint(point);
            }
        }

        public static Vector2[] ScreenVerticalPositions(int count)
        {
            var upper = new Vector2(Screen.width / 2f, Screen.height);
            var bottom = new Vector2(Screen.width / 2f, 0);

            float padding = 1.0f / (count + 2);
            float step = (1.0f - padding) / count;

            Camera cam = Camera.main;
            Vector2[] points = new Vector2[count];

            for (int i = 0; i < count; i++)
            {
                float t = padding + step * i;
                Vector2 point = Vector2.Lerp(upper, bottom, t);
                points[i] = cam.ScreenToWorldPoint(point);
            }

            return points;
        }

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

        public static float AngleFromTo(Vector2 from, Vector2 to)
        {
            Vector2 direction = to - from;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (angle < 0f) angle += 360f;
            return angle;
        }
    }
}
using Unity.Mathematics;
using UnityEngine;
using VoyagerApp.Lamps;
using VoyagerApp.Workspace;

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
                float t = UnityEngine.Random.value;
                Vector2 point = Vector2.Lerp(upper, bottom, t);
                return cam.ScreenToWorldPoint(point);
            }
        }

        public static float2[] ScreenVerticalPositions(int count)
        {
            var upper = new float2(Screen.width / 2f, Screen.height);
            var bottom = new float2(Screen.width / 2f, 0);

            float padding = 1.0f / (count + 2);
            float step = (1.0f - padding) / count;

            Camera cam = Camera.main;
            float2[] points = new float2[count];

            for (int i = 0; i < count; i++)
            {
                float t = padding + step * i;
                Vector2 point = Vector2.Lerp(upper, bottom, t);
                Vector3 vec = cam.ScreenToWorldPoint(point);
                points[i] = new float2(vec.x, vec.y);
            }

            return points;
        }

        public static float2[] ScreenHorizontalPositions(int count)
        {
            var left = new float2(0, Screen.height / 2.0f);
            var right = new float2(Screen.width, Screen.height / 2.0f);

            float padding = 1.0f / (count + 2);
            float step = (1.0f - padding) / count;

            Camera cam = Camera.main;
            float2[] points = new float2[count];

            for (int i = 0; i < count; i++)
            {
                float t = padding + step * i;
                Vector2 point = Vector2.Lerp(left, right, t);
                Vector3 vec = cam.ScreenToWorldPoint(point);
                points[i] = new float2(vec.x, vec.y);
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

        public static int2[] MapLampToVideoCoords(Lamp lamp, Texture2D frame)
        {
            if (lamp.mapping == null)
                lamp.mapping = new Videos.VideoPosition();

            int2[] coords = new int2[lamp.pixels];

            float2 p1 = new float2(lamp.mapping.x1, lamp.mapping.y1);
            float2 p2 = new float2(lamp.mapping.x2, lamp.mapping.y2);

            float2 delta = p2 - p1;
            float2 steps = delta / (coords.Length - 1);

            for (int i = 0; i < coords.Length; i++)
            {
                float x = p1.x + (steps.x * i);
                float y = p1.y + (steps.y * i);

                if (x > 1.0f || x < 0.0f || y > 1.0f || y < 0.0f)
                    coords[i] = new int2(-1, -1);
                else
                    coords[i] = new int2((int)(x * frame.width),
                                         (int)(y * frame.height));
            }

            return coords;
        }
    }
}
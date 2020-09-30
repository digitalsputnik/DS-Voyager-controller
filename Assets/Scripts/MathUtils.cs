using UnityEngine;

namespace VoyagerController
{
    public static class MathUtils
    {
        public static float AngleTo(this Vector2 from, Vector2 to) => AngleFromTo(from, to);

        public static float AngleFromTo(Vector2 from, Vector2 to)
        {
            var direction = to - from;
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (angle < 0f) angle += 360f;
            return angle;
        }
    }
}
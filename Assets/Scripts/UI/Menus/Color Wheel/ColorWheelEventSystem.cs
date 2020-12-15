using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VoyagerApp.Utilities;

namespace VoyagerApp.UI.Menus
{
    public class ColorWheelEventSystem : MonoBehaviour, IPointerClickHandler, IDragHandler
    {
        public event HueSaturationHandler onHueSaturationChanged;

        [SerializeField] RectTransform cursor       = null;
        [SerializeField] Vector2[] snappingPoints   = null;
        [SerializeField] Joystick joystick          = null;
        [SerializeField] float joystickSpeed        = 0.0f;
        [SerializeField] Image wheelImage           = null;

        RectTransform rect;
        Vector2 previousRectDimensions;

        void Start()
        {
            rect = GetComponent<RectTransform>();
            previousRectDimensions = new Vector2(rect.rect.width, rect.rect.height);
            StartCoroutine(LateStart());
        }

        IEnumerator LateStart()
        {
            yield return new WaitForSeconds(0.5f);
            SetupSnappingPoints();
        }

        void Update()
        {
            Vector2 delta = new Vector2(joystick.Horizontal, joystick.Vertical);
            if (delta.magnitude > 0.0001f)
            {
                delta *= joystickSpeed * Time.deltaTime;

                Vector2 pos = cursor.localPosition;
                pos += delta;
                pos = Vector2.ClampMagnitude(pos, rect.rect.width / 2.0f);
                cursor.localPosition = pos;

                CalculateHueAndSaturation();
            }
            CheckResize();
        }

        void CheckResize()
        {
            if (previousRectDimensions.x != rect.rect.width || previousRectDimensions.y != rect.rect.height)
            {
                SetupSnappingPoints();
                float marginX = rect.rect.width / previousRectDimensions.x;
                float marginY = rect.rect.height / previousRectDimensions.y;
                cursor.localPosition = new Vector2(cursor.localPosition.x * marginX, cursor.localPosition.y * marginY);
                previousRectDimensions = new Vector2(rect.rect.width, rect.rect.height);
            }
        }

        public void SetFromItsh(Itshe itshe)
        {
            float dist = itshe.s * rect.rect.width / 2.0f;
            float angle = 360.0f - (itshe.h * 360.0f - 90.0f);
            Vector3 position = Vector3.zero;
            position.x = 1 * Mathf.Cos(angle * Mathf.PI / 180) * dist;
            position.y = 1 * Mathf.Sin(angle * Mathf.PI / 180) * dist;
            cursor.localPosition = position;
            wheelImage.color = ColorUtils.ApplyTemperature(Color.white, itshe.t);
        }

        public void OnDrag(PointerEventData eventData)
        {
            float multiplier = rect.rect.width / ActualSize.x;
            Vector2 point = eventData.position - (Vector2)rect.position;
            SetCursorToClosestPoint(point * multiplier);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            float multiplier = rect.rect.width / ActualSize.x;
            Vector2 point = eventData.position - (Vector2)rect.position;
            SetCursorToClosestPoint(point * multiplier);
        }

        void SetupSnappingPoints()
        {
            float radius = rect.rect.width / 2.0f;
            float step = 1.0f / 12.0f;
            snappingPoints[0] = new Vector2(0.0f, 0.0f);

            int counter = 1;
            for (int a = 0; a < 12; a++)
            {
                for (int m = 0; m < 5; m++)
                {
                    float dist = radius * (8.0f / 9.0f);
                    float distJump = dist / 4;
                    float magnitude = (radius * 1.0f / 9.0f) + m * distJump;
                    float angle = (360 - (a * step * 360 - 90)) * Mathf.Deg2Rad;
                    float x = Mathf.Cos(angle);
                    float y = Mathf.Sin(angle);
                    Vector2 point = new Vector2(x, y) * magnitude;
                    snappingPoints[counter] = point;
                    counter++;
                }
            }
        }

        void SetCursorToClosestPoint(Vector2 position)
        {
            float distance = float.MaxValue;
            Vector2 closest = Vector2.zero;

            foreach (var point in snappingPoints)
            {
                float dist = Vector2.Distance(position, point);
                if (dist < distance)
                {
                    closest = point;
                    distance = dist;
                }
            }

            cursor.localPosition = closest;
            CalculateHueAndSaturation();
        }

        Vector2 ActualSize
        {
            get
            {
                var v = new Vector3[4];
                rect.GetWorldCorners(v);
                return new Vector2(v[3].x - v[0].x, v[1].y - v[0].y);
            }
        }

        void CalculateHueAndSaturation()
        {
            onHueSaturationChanged?.Invoke(Hue, Saturation);
        }


        float Saturation
        {
            get => cursor.localPosition.magnitude / rect.rect.width * 2.0f;
        }

        float Hue
        {
            get
            {
                var dir = cursor.localPosition;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
                if (angle < 0f) angle += 360f;
                return (360.0f - angle) / 360.0f;
            }
        }
    }

    public delegate void HueSaturationHandler(float hue, float saturation);
}

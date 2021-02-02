using System;
using System.Collections;
using DigitalSputnik.Colors;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VoyagerController.UI
{
    public class ColorWheelEventSystem : MonoBehaviour, IPointerClickHandler, IDragHandler
    {
        public event HueSaturationHandler OnHueSaturationChanged;

        [SerializeField] private RectTransform _cursor = null;
        [SerializeField] private Vector2[] _snappingPoints = null;
        [SerializeField] private Joystick _joystick = null;
        [SerializeField] private float _joystickSpeed = 0.0f;
        [SerializeField] private Image _wheelImage = null;

        private RectTransform _rect;
        private Vector2 _previousRectDimensions;

        private void Start()
        {
            _rect = GetComponent<RectTransform>();
            
            var rect = _rect.rect;
            _previousRectDimensions = new Vector2(rect.width, rect.height);
            
            StartCoroutine(LateStart());
        }

        private IEnumerator LateStart()
        {
            yield return new WaitForSeconds(0.2f);
            SetupSnappingPoints();
        }

        private void Update()
        {
            var delta = new Vector2(_joystick.Horizontal, _joystick.Vertical);
            if (delta.magnitude > 0.0001f)
            {
                delta *= _joystickSpeed * Time.deltaTime;

                Vector2 pos = _cursor.localPosition;
                pos += delta;
                pos = Vector2.ClampMagnitude(pos, _rect.rect.width / 2.0f);
                _cursor.localPosition = pos;

                CalculateHueAndSaturation();
            }
            CheckResize();
        }

        private void CheckResize()
        {
            if (!(Math.Abs(_previousRectDimensions.x - _rect.rect.width) > 0.0001f) &&
                !(Math.Abs(_previousRectDimensions.y - _rect.rect.height) > 0.0001f)) return;
            
            SetupSnappingPoints();
            
            var rect = _rect.rect;
            var marginX = rect.width / _previousRectDimensions.x;
            var marginY = rect.height / _previousRectDimensions.y;
            var localPosition = _cursor.localPosition;
            localPosition = new Vector2(localPosition.x * marginX, localPosition.y * marginY);
            
            _cursor.localPosition = localPosition;
            _previousRectDimensions = new Vector2(rect.width, rect.height);
        }

        public void SetFromItsh(Itshe itshe)
        {
            var dist = itshe.S * _rect.rect.width / 2.0f;
            var angle = 360.0f - (itshe.H * 360.0f - 90.0f);
            var position = Vector3.zero;
            position.x = 1 * Mathf.Cos(angle * Mathf.PI / 180) * dist;
            position.y = 1 * Mathf.Sin(angle * Mathf.PI / 180) * dist;
            _cursor.localPosition = position;
            _wheelImage.color = Temperature.Apply(Color.white, itshe.T);
        }

        public void OnDrag(PointerEventData eventData)
        {
            var multiplier = _rect.rect.width / ActualSize.x;
            var point = eventData.position - (Vector2)_rect.position;
            SetCursorToClosestPoint(point * multiplier);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var multiplier = _rect.rect.width / ActualSize.x;
            var point = eventData.position - (Vector2)_rect.position;
            SetCursorToClosestPoint(point * multiplier);
        }

        private void SetupSnappingPoints()
        {
            var radius = _rect.rect.width / 2.0f;
            var step = 1.0f / 12.0f;
            var counter = 1;
            
            _snappingPoints = new Vector2[12 * 5 + 1];
            _snappingPoints[0] = new Vector2(0.0f, 0.0f);

            for (var a = 0; a < 12; a++)
            {
                for (var m = 0; m < 5; m++)
                {
                    var dist = radius * (8.0f / 9.0f);
                    var distJump = dist / 4;
                    var magnitude = (radius * 1.0f / 9.0f) + m * distJump;
                    var angle = (360 - (a * step * 360 - 90)) * Mathf.Deg2Rad;
                    var x = Mathf.Cos(angle);
                    var y = Mathf.Sin(angle);
                    var point = new Vector2(x, y) * magnitude;
                    _snappingPoints[counter] = point;
                    counter++;
                }
            }
        }

        private void SetCursorToClosestPoint(Vector2 position)
        {
            var distance = float.MaxValue;
            var closest = Vector2.zero;

            foreach (var point in _snappingPoints)
            {
                var dist = Vector2.Distance(position, point);
                if (!(dist < distance)) continue;
                
                closest = point;
                distance = dist;
            }

            _cursor.localPosition = closest;
            CalculateHueAndSaturation();
        }

        private Vector2 ActualSize
        {
            get
            {
                var v = new Vector3[4];
                _rect.GetWorldCorners(v);
                return new Vector2(v[3].x - v[0].x, v[1].y - v[0].y);
            }
        }

        private void CalculateHueAndSaturation()
        {
            OnHueSaturationChanged?.Invoke(Hue, Saturation);
        }


        private float Saturation => _cursor.localPosition.magnitude / _rect.rect.width * 2.0f;

        private float Hue
        {
            get
            {
                var dir = _cursor.localPosition;
                var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
                if (angle < 0f) angle += 360f;
                return (360.0f - angle) / 360.0f;
            }
        }
    }

    public delegate void HueSaturationHandler(float hue, float saturation);
}

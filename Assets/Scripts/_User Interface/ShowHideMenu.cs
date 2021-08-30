using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VoyagerController.UI
{
    public class ShowHideMenu : MonoBehaviour
    {
        [Header("General")]
        [SerializeField] private float _speed = 0.0f;
        [SerializeField] private Vector2 _openPosition = Vector2.zero;
        [SerializeField] private Vector2 _closePosition = Vector2.zero;
        [SerializeField] private RectTransform _target = null;
        [SerializeField] private bool _open = true;
        [Header("Icon")]
        [SerializeField] private Sprite _showSprite = null;
        [SerializeField] private Sprite _hideSprite = null;
        [SerializeField] private Image _iconImage = null;
        
        public Vector2 OpenPosition
        {
            get => _openPosition;
            set => _openPosition = value;
        }
        
        public Vector2 ClosePosition
        {
            get => _closePosition;
            set => _closePosition = value;
        }

        public float Speed
        {
            get => _speed;
            set => _speed = value;
        }

        private void Start()
        {
            Open = _open;
            StopAllCoroutines();
            var dest = _open ? _openPosition : _closePosition;
            _target.anchoredPosition = dest;
        }

        public bool Open
        {
            get => _open;
            set
            {
                _open = value;
                _iconImage.sprite = _open ? _hideSprite : _showSprite;
                PlayAnimation();
            }
        }

        public void Toggle()
        {
            Open = !Open;
        }

        private void PlayAnimation()
        {
            StopAllCoroutines();
            StartCoroutine(EnumPlayAnimation());
        }

        private IEnumerator EnumPlayAnimation()
        {
            var startTime = Time.time;
            var startPosition = _target.anchoredPosition;
            var destPosition = _open ? _openPosition : _closePosition;

            while (_target.anchoredPosition != destPosition)
            {
                var passed = Time.time - startTime;
                var time = passed / _speed;
                _target.anchoredPosition = Vector2.Lerp(startPosition,destPosition, time);
                yield return null;
            }
        }
    }
}
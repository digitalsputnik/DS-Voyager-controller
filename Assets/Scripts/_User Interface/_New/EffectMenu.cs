using System.Collections.Generic;
using System.Linq;
using DigitalSputnik;
using DigitalSputnik.Voyager;
using UnityEngine;
using VoyagerController.Effects;

namespace VoyagerController.UI
{
    public class EffectMenu : MonoBehaviour
    {
        private const float EFFECT_UPDATE_INTERVAL = 0.5f;
        
        public bool IsOpen { get; private set; }

        [Header("Items")]
        [SerializeField] private EffectMenuItem _itemPrefab;
        [SerializeField] private Transform _itemsContainer;
        [SerializeField] private Transform _addEffectButton;
        
        [Header("Show & Hide")]
        [SerializeField] private RectTransform _effectMenuContainer;
        [SerializeField] private CanvasGroup _effectMenuCanvas;
        [SerializeField] private float _hidePosition = -360.0f;
        [SerializeField] private float _hideAlpha = 0.3f;
        [SerializeField] private float _tweenTime = 0.3f;
        
        private readonly Dictionary<Effect, EffectMenuItem> _effectItems = new Dictionary<Effect, EffectMenuItem>();

        private void Start()
        {
            InvokeRepeating(nameof(UpdateEffectList), 0.0f, EFFECT_UPDATE_INTERVAL);
            
            _effectMenuContainer.anchoredPosition = new Vector2(_hidePosition, _effectMenuContainer.anchoredPosition.x);
            _effectMenuCanvas.alpha = _hideAlpha;
            IsOpen = false;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.O)) Show();
            if (Input.GetKeyDown(KeyCode.H)) Hide();
        }

        public void Show()
        {
            LeanTween.moveLocalX(_effectMenuContainer.gameObject, 0.0f, _tweenTime)
                .setEaseOutCubic();
            LeanTween.value(_effectMenuContainer.gameObject, _effectMenuCanvas.alpha, 1.0f, _tweenTime)
                .setOnComplete(() => IsOpen = true)
                .setOnUpdate(AlphaChanged)
                .setEaseOutCubic();
        }

        private void AlphaChanged(float value) => _effectMenuCanvas.alpha = value;

        public void Hide()
        {
            LeanTween.moveLocalX(_effectMenuContainer.gameObject, _hidePosition, _tweenTime)
                .setEaseOutCubic();
            LeanTween.value(_effectMenuContainer.gameObject, _effectMenuCanvas.alpha, _hideAlpha, _tweenTime)
                .setOnComplete(() => IsOpen = false)
                .setOnUpdate(AlphaChanged)
                .setEaseOutCubic();
        }

        private void UpdateEffectList()
        {
            var activeLamps = GetActiveLamps();
            
            foreach (var lamp in activeLamps)
            {
                var effect = Metadata.Get<LampData>(lamp.Serial).Effect;
                if (!_effectItems.ContainsKey(effect))
                    AddNewEffectToList(effect);
            }

            foreach (var effect in _effectItems.Keys)
            {
                if (activeLamps.All(l => Metadata.Get<LampData>(l.Serial).Effect != effect))
                    RemoveEffectFromList(effect);
            }
            
            _addEffectButton.gameObject.SetActive(activeLamps.Count != 0);
        }

        private void AddNewEffectToList(Effect effect)
        {
            var item = Instantiate(_itemPrefab, _itemsContainer);
            item.SetEffect(effect);
            _effectItems.Add(effect, item);
            _addEffectButton.SetSiblingIndex(_itemsContainer.childCount - 1);
        }

        private void RemoveEffectFromList(Effect effect)
        {
            Destroy(_effectItems[effect]);
            _effectItems.Remove(effect);
        }

        private static List<VoyagerLamp> GetActiveLamps()
        {
            return LampManager.Instance
                .GetLampsOfType<VoyagerLamp>()
                .Where(lamp => Metadata.Get<LampData>(lamp.Serial).Effect != null)
                .ToList();
        }
    }
}
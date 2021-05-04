using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace VoyagerController.UI
{
    public class ListPickerMenu : Menu, IPointerDownHandler, IPointerUpHandler
    {
        public static ListPickerMenu instance;
        void Awake() => instance = this;

        [SerializeField] Text titleText = null;
        [SerializeField] RectTransform itemsContainer = null;
        [SerializeField] Text itemPrefab = null;
        [SerializeField] float itemHeight = 0.0f;
        [SerializeField] float correctionSpeed = 5.0f;
        [SerializeField] Color activeColor = Color.white;
        [SerializeField] Color deactiveColor = Color.gray;

        List<Text> items = new List<Text>();
        Action<int> onPick;
        int startIndex;
        int index;

        float areaHeight;
        float areaClamped;

        ControlState state = ControlState.ByView;
        float startYPos;
        float startYPointerPos;
        int pointerID;

        enum ControlState { ByUser, ByView }

        public void PickValue(string title, int index, string[] items, Action<int> onPick)
        {
            titleText.text = title;

            this.index = index;
            this.onPick = onPick;

            startIndex = index;

            ClearItems();
            AddItems(items);

            areaHeight = items.Length * itemHeight;
            areaClamped = areaHeight - itemHeight;

            YPos = PositionOfIndex(index);
            SetItemActive(index);

            Open = true;
        }

        public void Pick()
        {
            onPick?.Invoke(index);
            onPick = null;
            Open = false;
        }

        public void Cancel()
        {
            int val = Mathf.Clamp(startIndex, 0, items.Count);
            onPick?.Invoke(val);
            onPick = null;
            Open = false;
        }

        void AddItems(string[] itemTexts)
        {
            foreach (var itemText in itemTexts)
            {
                Text item = Instantiate(itemPrefab, itemsContainer);
                item.alignment = TextAnchor.MiddleCenter;
                item.text = itemText;
                items.Add(item);
            }
        }

        void ClearItems()
        {
            foreach (var item in items.ToArray())
            {
                items.Remove(item);
                Destroy(item.gameObject);
            }
        }

        void Update()
        {
            if (state == ControlState.ByView)
            {
                float time = correctionSpeed * Time.deltaTime;
                YPos = Mathf.Lerp(YPos, PositionOfIndex(index), time);
            }
            else
            {
                float delta = startYPointerPos - GetScreenYPos();
                YPos = startYPos - delta;
                SetItemActive(IndexOfPosition(YPos));
            }
        }

        float PositionOfIndex(int i)
        {
            float value = areaClamped;
            value -= i * itemHeight;
            value -= areaClamped / 2.0f;
            value *= -1.0f;
            return value;
        }

        int IndexOfPosition(float y)
        {
            List<float> positions = new List<float>();
            for (int i = 0; i < items.Count; i++)
                positions.Add(PositionOfIndex(i));
            float closest = positions.OrderBy(p => Mathf.Abs(p - y)).First();
            return positions.IndexOf(closest);
        }

        float YPos
        {
            get => itemsContainer.localPosition.y;
            set => itemsContainer.localPosition = new Vector2(0.0f, value);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            state = ControlState.ByUser;
            startYPos = YPos;
            startYPointerPos = eventData.position.y;
            pointerID = eventData.pointerId;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId == pointerID)
            {
                index = IndexOfPosition(YPos);
                SetItemActive(index);
                state = ControlState.ByView;
            }
        }

        void SetItemActive(int i)
        {
            foreach (var item in items)
                item.color = deactiveColor;
            items[i].color = activeColor;
        }

        float GetScreenYPos()
        {
            if (Application.isMobilePlatform)
            {
                Touch touch = Input.touches.FirstOrDefault(t => t.fingerId == pointerID);
                return touch.position.y;
            }

            return Input.mousePosition.y;
        }
    }
}
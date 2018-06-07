/* 
*   NatCam
*   Copyright (c) 2018 Yusuf Olokoba
*/

namespace NatCamU.Examples {

    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.EventSystems;
    using Core;

    [RequireComponent(typeof(EventTrigger), typeof(Graphic))]
    public sealed class Focuser : MonoBehaviour, IPointerUpHandler {
        
        Vector3[] corners = new Vector3[4]; 

        void IPointerUpHandler.OnPointerUp (PointerEventData eventData) {
            // Do nothing if camera isn't playing
            if (!NatCam.IsPlaying) return;
            Vector3 worldPoint;
            RectTransform transform = this.transform as RectTransform;
            if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(transform, eventData.pressPosition, eventData.pressEventCamera, out worldPoint)) return;
            transform.GetWorldCorners(corners);
            var point = worldPoint - corners[0];
            var size = new Vector2(corners[3].x, corners[1].y) - (Vector2)corners[0];
            Vector2 relativePoint = new Vector2(point.x / size.x, point.y / size.y);
            NatCam.Camera.FocusPoint = relativePoint;
        }
    }
}
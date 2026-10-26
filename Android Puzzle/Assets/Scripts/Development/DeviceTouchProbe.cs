using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Puzzle.Development
{
    public sealed class DeviceTouchProbe : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public Text statusLabel;
        public Text eventLabel;
        public RectTransform marker;
        private int presses;
        private string inputSource = "Waiting for input";
        private int activePointer = int.MinValue;
        private int previousTouchCount = -1;

        private void Update()
        {
            int count = 0;
            foreach (var device in InputSystem.devices)
                if (device is Touchscreen screen)
                    foreach (var touch in screen.touches)
                        if (touch.press.isPressed) count++;
            if (count == previousTouchCount) return;
            previousTouchCount = count;
            statusLabel.text = $"{inputSource}\nActive touches: {count}   |   Presses: {presses}";
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            presses++;
            activePointer = eventData.pointerId;
            inputSource = eventData is ExtendedPointerEventData input && input.device != null
                ? (input.device.remote ? "Unity Remote touch" : input.device.displayName)
                : "Pointer";
            previousTouchCount = -1;
            MoveMarker(eventData, "DOWN");
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId == activePointer) MoveMarker(eventData, "DRAG");
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointer) return;
            MoveMarker(eventData, "UP");
            activePointer = int.MinValue;
        }

        private void MoveMarker(PointerEventData eventData, string phase)
        {
            var rect = (RectTransform)transform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, eventData.position, eventData.pressEventCamera, out var local))
                return;
            local.x = Mathf.Clamp(local.x, rect.rect.xMin + 24, rect.rect.xMax - 24);
            local.y = Mathf.Clamp(local.y, rect.rect.yMin + 24, rect.rect.yMax - 24);
            marker.anchoredPosition = local;
            eventLabel.text = $"{phase}   x: {eventData.position.x:0}   y: {eventData.position.y:0}";
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityEngine.EventSystems {

    public class HandEventData : PointerEventData {

        public Vector3 pointerFingerPosition;
        public Vector3 handPosition;
        public bool isPressed = false;
        public bool isReleased = false;
        public Vector2 swipeStart;

        public HandEventData(EventSystem eventSystem)
            : base(eventSystem) {
        }

        public override string ToString() {
            var sb = new StringBuilder();
            sb.AppendLine("<b>Position</b>: " + position);
            sb.AppendLine("<b>delta</b>: " + delta);
            sb.AppendLine("<b>eligibleForClick</b>: " + eligibleForClick);
            sb.AppendLine("<b>pointerEnter</b>: " + pointerEnter);
            sb.AppendLine("<b>pointerPress</b>: " + pointerPress);
            sb.AppendLine("<b>lastPointerPress</b>: " + lastPress);
            sb.AppendLine("<b>pointerDrag</b>: " + pointerDrag);
            sb.AppendLine("<b>pointerFingerPosition</b>: " + pointerFingerPosition);
            sb.AppendLine("<b>handPosition</b>: " + handPosition);
            sb.AppendLine("<b>swipeStart</b>: " + swipeStart);
            sb.AppendLine("<b>Use Drag Threshold</b>: " + useDragThreshold);
            sb.AppendLine("<b>isPressed</b>: " + isPressed);
            sb.AppendLine("<b>isReleased</b>: " + isReleased);
            return sb.ToString();
        }

    }

    public static class HandEventDataExtension {

        public static bool IsHandPointer(this PointerEventData pointerEventData) {
            return (pointerEventData is HandEventData);
        }

        public static void SetSwipeStart(this PointerEventData pointerEventData, Vector2 start) {
            HandEventData handEventData = pointerEventData as HandEventData;
            Assert.IsNotNull(handEventData);

            handEventData.swipeStart = start;
        }

    }
}

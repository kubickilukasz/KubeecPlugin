using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Kubeec.VR.Interactions;

namespace Kubeec.VR.Player{
    public class InputModule : PointerInputModule {

        public float angleDragThreshold = 1;
        [NonSerialized] public List<HandController> activeHands = new();
        [NonSerialized] public HandGraphicRaycaster currentGraphicRaycaster;

        readonly MouseState mouseState = new MouseState();

        bool isActive => activeHands != null && activeHands.Count > 0;

        void LateUpdate() {
            if (isActive) {
                currentGraphicRaycaster.ResetBeforeRaycast();
                for (int i = 0; i < activeHands.Count; i++) {
                    ProcessMouseEvent(GetCanvasPointerData());
                }
            } else {
                ClearSelection();
            }
        }

        public override void Process() {
        }

        public override void DeactivateModule() {
            base.DeactivateModule();
            ClearSelection();
        }

        protected MouseState GetCanvasPointerData() {
            // Get the OVRRayPointerEventData reference
            HandEventData leftData;
            GetPointerData(kMouseLeftId, out leftData);
            leftData.Reset();

            //Now set the world space ray. This ray is what the user uses to point at UI elements
            //leftData.worldSpaceRay = new Ray(rayTransform.position, rayTransform.forward);
            leftData.scrollDelta = GetExtraScrollDelta();

            //Populate some default values
            leftData.button = PointerEventData.InputButton.Left;
            leftData.useDragThreshold = true;
            // Perform raycast to find intersections with world
            eventSystem.RaycastAll(leftData, m_RaycastResultCache);
            //raycaster.Raycast(leftData, m_RaycastResultCache);
            RaycastResult raycast = FindFirstRaycast(m_RaycastResultCache);
            leftData.pointerCurrentRaycast = raycast;
            m_RaycastResultCache.Clear();
            if (currentGraphicRaycaster) {
                leftData.position = currentGraphicRaycaster.GetScreenPosition(raycast);
            }

            // copy the apropriate data into right and middle slots
            HandEventData rightData;
            GetPointerData(kMouseRightId, out rightData);
            CopyFromTo(leftData, rightData);
            rightData.button = PointerEventData.InputButton.Right;

            HandEventData middleData;
            GetPointerData(kMouseMiddleId, out middleData);
            CopyFromTo(leftData, middleData);
            middleData.button = PointerEventData.InputButton.Middle;


            mouseState.SetButtonState(PointerEventData.InputButton.Left,
                GetGazeButtonState(leftData), leftData);
            mouseState.SetButtonState(PointerEventData.InputButton.Right,
                PointerEventData.FramePressState.NotChanged, rightData);
            mouseState.SetButtonState(PointerEventData.InputButton.Middle,
                PointerEventData.FramePressState.NotChanged, middleData);
            return mouseState;
        }

        protected new void ClearSelection() {
            var baseEventData = GetBaseEventData();

            foreach (var pointer in m_PointerData.Values) {
                // clear all selection
                HandlePointerExitAndEnter(pointer, null);
            }

            m_PointerData.Clear();
            eventSystem.SetSelectedGameObject(null, baseEventData);
        }

        protected Dictionary<int, HandEventData> handEventDatas = new Dictionary<int, HandEventData>();
        protected bool GetPointerData(int id, out HandEventData data) {
            if (!handEventDatas.TryGetValue(id, out data)) {
                data = new HandEventData(eventSystem) {
                    pointerId = id,
                };

                handEventDatas.Add(id, data);
                return true;
            }
            return false;
        }

        protected Vector2 SwipeAdjustedPosition(Vector2 originalPosition, PointerEventData pointerEvent) {
            // On android we use the touchpad position (accessed through Input.mousePosition) to modify
            // the effective cursor position for events related to dragging. This allows the user to
            // use the touchpad to drag draggable UI elements
            //if (useSwipeScroll)
            //{
            //    Vector2 delta = (Vector2)Input.mousePosition - pointerEvent.GetSwipeStart();
            //    if (InvertSwipeXAxis)
            //        delta.x *= -1;
            //    return originalPosition + delta * swipeDragScale;
            //}
            return originalPosition;
        }

        protected Vector2 GetExtraScrollDelta() {
            Vector2 scrollDelta = new Vector2();
            //if (useRightStickScroll) {
            //    Vector2 s = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
            //    if (Mathf.Abs(s.x) < rightStickDeadZone) s.x = 0;
            //    if (Mathf.Abs(s.y) < rightStickDeadZone) s.y = 0;
            //    scrollDelta = s;
            //}

            return scrollDelta;
        }

        protected override void ProcessDrag(PointerEventData pointerEvent) {
            Vector2 originalPosition = pointerEvent.position;
            bool moving = true;
            if (moving && pointerEvent.pointerDrag != null
                       && !pointerEvent.dragging
                       && ShouldStartDrag(pointerEvent)) {

                pointerEvent.position = SwipeAdjustedPosition(originalPosition, pointerEvent);
                

                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.beginDragHandler);
                pointerEvent.dragging = true;
            }

            // Drag notification
            if (pointerEvent.dragging && moving && pointerEvent.pointerDrag != null) {
                 pointerEvent.position = SwipeAdjustedPosition(originalPosition, pointerEvent);
                
                // Before doing drag we should cancel any pointer down state
                // And clear selection!
                if (pointerEvent.pointerPress != pointerEvent.pointerDrag) {
                    ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

                    pointerEvent.eligibleForClick = false;
                    pointerEvent.pointerPress = null;
                    pointerEvent.rawPointerPress = null;
                }

                ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.dragHandler);
            }
        }

        protected PointerEventData.FramePressState GetGazeButtonState(HandEventData eventData) {
            bool pressed = eventData.isPressed;
            bool released = eventData.isReleased;

            if (pressed && released)
                return PointerEventData.FramePressState.PressedAndReleased;
            if (pressed)
                return PointerEventData.FramePressState.Pressed;
            if (released)
                return PointerEventData.FramePressState.Released;
            return PointerEventData.FramePressState.NotChanged;
        }

        protected void CopyFromTo(HandEventData @from, HandEventData @to) {
            @to.position = @from.position;
            @to.delta = @from.delta;
            @to.scrollDelta = @from.scrollDelta;
            @to.pointerCurrentRaycast = @from.pointerCurrentRaycast;
            @to.pointerEnter = @from.pointerEnter;
            @to.pointerFingerPosition = @from.pointerFingerPosition;
            @to.handPosition = @from.handPosition;
            @to.isPressed = @from.isPressed;
            @to.isReleased = @from.isReleased;
        }

        bool ShouldStartDrag(PointerEventData pointerEvent) {
            if (!pointerEvent.useDragThreshold)
                return true;

            Vector3 cameraPos = pointerEvent.pressEventCamera.transform.position;
            Vector3 pressDir = (pointerEvent.pointerPressRaycast.worldPosition - cameraPos).normalized;
            Vector3 currentDir = (pointerEvent.pointerCurrentRaycast.worldPosition - cameraPos).normalized;
            return Vector3.Dot(pressDir, currentDir) < Mathf.Cos(Mathf.Deg2Rad * (angleDragThreshold));
        }

        void ProcessMouseEvent(MouseState mouseData) {
            var pressed = mouseData.AnyPressesThisFrame();
            var released = mouseData.AnyReleasesThisFrame();

            var leftButtonData = mouseData.GetButtonState(PointerEventData.InputButton.Left).eventData;

            // Process the first mouse button fully
            ProcessMousePress(leftButtonData);
            ProcessMove(leftButtonData.buttonData);
            ProcessDrag(leftButtonData.buttonData);

            // Now process right / middle clicks
            //ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData);
            //ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Right).eventData.buttonData);
            //ProcessMousePress(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData);
            //ProcessDrag(mouseData.GetButtonState(PointerEventData.InputButton.Middle).eventData.buttonData);

            if (!Mathf.Approximately(leftButtonData.buttonData.scrollDelta.sqrMagnitude, 0.0f)) {
                var scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(
                    leftButtonData.buttonData.pointerCurrentRaycast.gameObject);
                ExecuteEvents.ExecuteHierarchy(scrollHandler, leftButtonData.buttonData, ExecuteEvents.scrollHandler);
            }
        }

        void ProcessMousePress(MouseButtonEventData data) {
            var pointerEvent = data.buttonData;
            var currentOverGo = pointerEvent.pointerCurrentRaycast.gameObject;

            // PointerDown notification
            if (data.PressedThisFrame()) {
                pointerEvent.eligibleForClick = true;
                pointerEvent.delta = Vector2.zero;
                pointerEvent.dragging = false;
                pointerEvent.useDragThreshold = true;
                pointerEvent.pressPosition = pointerEvent.position;
                //pointerEvent.SetSwipeStart(Input.mousePosition);

                pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;

                DeselectIfSelectionChanged(currentOverGo, pointerEvent);

                // search for the control that will receive the press
                // if we can't find a press handler set the press
                // handler to be what would receive a click.
                var newPressed =
                    ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.pointerDownHandler);

                // didnt find a press handler... search for a click handler
                if (newPressed == null)
                    newPressed = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

                float time = Time.unscaledTime;

                if (newPressed == pointerEvent.lastPress) {
                    var diffTime = time - pointerEvent.clickTime;
                    if (diffTime < 0.3f)
                        ++pointerEvent.clickCount;
                    else
                        pointerEvent.clickCount = 1;

                    pointerEvent.clickTime = time;
                } else {
                    pointerEvent.clickCount = 1;
                }

                pointerEvent.pointerPress = newPressed;
                pointerEvent.rawPointerPress = currentOverGo;

                pointerEvent.clickTime = time;

                // Save the drag handler as well
                pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(currentOverGo);

                if (pointerEvent.pointerDrag != null)
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent,
                        ExecuteEvents.initializePotentialDrag);
            }

            // PointerUp notification
            if (data.ReleasedThisFrame()) {
                ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);

                // see if we mouse up on the same element that we clicked on...
                var pointerUpHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);

                // PointerClick and Drop events
                if (pointerEvent.pointerPress == pointerUpHandler && pointerEvent.eligibleForClick) {
                    ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerClickHandler);
                } else if (pointerEvent.pointerDrag != null) {
                    ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
                }

                pointerEvent.eligibleForClick = false;
                pointerEvent.pointerPress = null;
                pointerEvent.rawPointerPress = null;

                if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
                    ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);

                pointerEvent.dragging = false;
                pointerEvent.pointerDrag = null;

                // redo pointer enter / exit to refresh state
                // so that if we moused over somethign that ignored it before
                // due to having pressed on something else
                // it now gets it.
                if (currentOverGo != pointerEvent.pointerEnter) {
                    HandlePointerExitAndEnter(pointerEvent, null);
                    HandlePointerExitAndEnter(pointerEvent, currentOverGo);
                }
            }
        }

    }

}

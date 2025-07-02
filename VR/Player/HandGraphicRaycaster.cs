using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using Kubeec.VR.Interactions;
using System.Linq;

namespace Kubeec.VR.Player {

    [RequireComponent(typeof(Canvas))]
    public class HandGraphicRaycaster : GraphicRaycaster {

        [NonSerialized] Canvas _canvas;
        public Canvas canvas {
            get {
                if (_canvas != null)
                    return _canvas;

                _canvas = GetComponent<Canvas>();
                return _canvas;
            }
        }

        [NonSerialized] RectTransform _rectTransform;
        public RectTransform rectTransform {
            get {
                if (_rectTransform != null)
                    return _rectTransform;

                _rectTransform = GetComponent<RectTransform>();
                return _rectTransform;
            }
        }

        public override Camera eventCamera => currentCamera;

        Camera currentCamera;
        Dictionary<HandController, OnRaycastAction> registeredHandController = new();
        List<HandController> registeredHandControllerList = new();
        bool wasPressedByTouch = false;
        int indexPerFrame = 0;

        public void ResetBeforeRaycast() {
            indexPerFrame = 0;
        }

        public void Register(HandController handController, OnRaycastAction onRaycast = null) {
            if (!registeredHandController.ContainsKey(handController)) {
                registeredHandController.Add(handController, onRaycast);
                InputModule inputModule = EventSystem.current.currentInputModule as InputModule;
                currentCamera = handController.CurrentCamera;
                if (inputModule != null) {
                    inputModule.currentGraphicRaycaster = this;
                    inputModule.activeHands = registeredHandControllerList = registeredHandController.Keys.ToList();
                }
            }
        }

        public void Unregister(HandController handController) {
            if (registeredHandController.ContainsKey(handController)) {
                registeredHandController.Remove(handController);
                InputModule inputModule = EventSystem.current.currentInputModule as InputModule;
                if (inputModule != null) {
                    inputModule.activeHands = registeredHandControllerList = registeredHandController.Keys.ToList();
                }
            }
        }

        public Vector2 GetScreenPosition(RaycastResult raycastResult) {
            if (currentCamera) {
                return currentCamera.WorldToScreenPoint(raycastResult.worldPosition);
            }
            return Vector2.zero;
        }

        public Vector3 GetCanvasPosition(Vector3 worldPosition) {
            return rectTransform.InverseTransformPoint(worldPosition) + new Vector3(rectTransform.sizeDelta.x / 2, rectTransform.sizeDelta.y / 2, 0);
        }

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList) {
            if (eventData.IsHandPointer()) {
                Raycast(eventData as HandEventData, resultAppendList);
            }
        }

        public void Raycast(HandEventData eventData, List<RaycastResult> resultAppendList) {
            if (registeredHandController.Count <= indexPerFrame) {
                return;
            }
            HandController handController = registeredHandControllerList[indexPerFrame];
            eventData.pointerFingerPosition = handController.TopPointFingerPosition;
            eventData.handPosition = handController.CurrentHandPosition;
            eventData.isReleased = handController.IsSelectUp();
            eventData.isPressed = handController.IsSelectDown();

            Vector3 localFingerPosition = GetCanvasPosition(eventData.pointerFingerPosition);
            Vector3 localHandPosition = GetCanvasPosition(eventData.handPosition);
            bool isPressedByTouch = (localHandPosition.z > 0 && localFingerPosition.z <= 0) || (localHandPosition.z < 0 && localFingerPosition.z >= 0);
            eventData.isPressed |= (isPressedByTouch && !wasPressedByTouch);
            eventData.isReleased |= (wasPressedByTouch && !isPressedByTouch);
            wasPressedByTouch = isPressedByTouch;
            GetRaycastedUIObjects(eventData.pointerFingerPosition, localFingerPosition, resultAppendList, out RaycastResult result);
            registeredHandController[handController].Invoke(handController, result, handController.IsSelectPressed() || isPressedByTouch);
            indexPerFrame++;
        }

        void GetRaycastedUIObjects(Vector3 worldPosition, Vector2 canvasPosition, List<RaycastResult> results, out RaycastResult lastResult) {
            if (currentCamera == null || canvasPosition.x < 0 || canvasPosition.y < 0 || canvasPosition.x > rectTransform.sizeDelta.x || canvasPosition.y > rectTransform.sizeDelta.y) {
                lastResult = new();
                return;
            }
            IList<Graphic> graphics = GraphicRegistry.GetRaycastableGraphicsForCanvas(canvas);
            int totalCount = graphics.Count;

            Vector3 posOnScreen = currentCamera.WorldToScreenPoint(worldPosition);
            List<RaycastResult> tempResults = new();
            for (int i = 0; i < totalCount; ++i) {
                Graphic graphic = graphics[i];

                // -1 means it hasn't been processed by the canvas, which means it isn't actually drawn
                if (!graphic.raycastTarget || graphic.canvasRenderer.cull || graphic.depth == -1) // we don't need this if because we have the next if statement
                    continue;

                Vector3[] corners = new Vector3[4];
                graphic.rectTransform.GetWorldCorners(corners);
                for (int j = 0; j < corners.Length; j++) {
                    corners[j] = GetCanvasPosition(corners[j]);
                }
                if (canvasPosition.x < corners[0].x || canvasPosition.x > corners[2].x) {
                    continue;
                }
                if (canvasPosition.y < corners[0].y || canvasPosition.y > corners[2].y) {
                    continue;
                }

                if (!graphic.Raycast(posOnScreen, currentCamera)) {
                    continue;
                }

                RaycastResult castResult = new RaycastResult {
                    gameObject = graphic.gameObject,
                    module = this,
                    distance = 0f, //TODO
                    worldNormal = -transform.forward,
                    index = results.Count,
                    depth = graphic.depth,
                    worldPosition = worldPosition,
                    screenPosition = canvasPosition
                };

                tempResults.Add(castResult);
            }
            if (tempResults.Count > 0) {
                tempResults.Sort((g1, g2) => g2.depth.CompareTo(g1.depth));
                results.AddRange(tempResults);
                lastResult = tempResults[tempResults.Count - 1];
            } else {
                lastResult = new();
            }
        }

    }


    public delegate void OnRaycastAction(HandController controller, RaycastResult result, bool isPressed);

}


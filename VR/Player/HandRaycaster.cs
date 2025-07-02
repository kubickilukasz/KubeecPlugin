using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Kubeec.VR.Outline;
using Kubeec.VR.Interactions;
using Kubeec.VR.Character;

namespace Kubeec.VR.Player {

    public class HandRaycaster : Initable<HandRaycaster.HandRaycastData> {

        [SerializeField] CharacterHandTracker handTracker;
        [SerializeField] HandController handController;
        [SerializeField] HandInteractor interactionHand;
        [SerializeField] HandPointer pointerPrefab;
        [SerializeField] OutlineController outlineController;
        [SerializeField] LineRenderer lineRenderer;

        [Space]

        [SerializeField] bool forceFingerToCanvas = true;
        [SerializeField] float maxDistanceToFreeMove = 0.01f;
        [SerializeField] LayerMask layerMaskItem;
        [SerializeField] LayerMask layerMaskOther;
        [SerializeField] float maxDistanceToItem = 10f;
        [SerializeField] float radiusToItem = 2f;
        [SerializeField] float forceToItem = 100f;

        public Transform PointFinger { set; get; }

        HandPointer pointer;
        HandGraphicRaycaster currentGraphicRaycaster;
        IMoveable currentRaycastedtAttractable;
        IOutlineable outlineableAttractable;
        bool initCanRaycast = false;
        bool runtimeCanRaycast = true;
        float squareRadiusToItem;
        RaycastHit hit;
        Vector3 force;

        bool canRaycast => initCanRaycast && runtimeCanRaycast;

        protected void OnDestroy() {
            if (pointer != null) {
                Destroy(pointer.gameObject);
            }
        }

        public void StartUIRaycast(HandGraphicRaycaster graphicRaycaster) {
            if (!canRaycast) return;

            if (graphicRaycaster != currentGraphicRaycaster) {
                StopUIRaycast(currentGraphicRaycaster);
                currentGraphicRaycaster = graphicRaycaster;
                graphicRaycaster.Register(handController, OnRaycastUI);
            }
        }

        public void StopUIRaycast(HandGraphicRaycaster graphicRaycaster) {
            if (!canRaycast) return;
            if (currentGraphicRaycaster != graphicRaycaster || currentGraphicRaycaster == null) {
                return;
            }
            graphicRaycaster.Unregister(handController);
            handController.StopMagicBarrier();
            currentGraphicRaycaster = null;
            ResetUIRaycast();
        }

        public void SetCanRaycast(bool value) {
            runtimeCanRaycast = value;
            if (!runtimeCanRaycast) {
                ResetCurrentRaycastedAttractable();
            }
        }

        public void RaycastItem() {
            if (!canRaycast || handController.IsGripPressed()) {
                ResetCurrentRaycastedAttractable();
                return;
            }
            Quaternion rotation = handController.GetInputHandRotation();
            Vector3 direction = (rotation * Vector3.forward).normalized;
            Ray ray = new Ray(handController.GetInputHandPosition() + (direction * radiusToItem), direction);
            bool wasHit = true;
            wasHit = Physics.SphereCast(ray, radiusToItem, out hit, maxDistanceToItem, layerMaskItem);
            if (wasHit && hit.transform.TryGetComponentFromSource(out IMoveable attractable)) {
                if (Physics.Raycast(hit.transform.position, -direction, Vector3.Distance(hit.transform.position, ray.origin), layerMaskOther)) {
                    ResetCurrentRaycastedAttractable();
                    return;
                }
                if (attractable.CanMove()) {
                    currentRaycastedtAttractable = attractable;
                    outlineableAttractable = attractable as IOutlineable;
                    if (outlineableAttractable != null) {
                        outlineController.StartOutline(outlineableAttractable, this);
                    }
                }
            } else {
                ResetCurrentRaycastedAttractable();
            }
        }

        public bool MoveAttractableToHand() {
            if (currentRaycastedtAttractable == null) {
                return false;
            }
            if (!currentRaycastedtAttractable.CanMove()) {
                ResetCurrentRaycastedAttractable();
                return false;
            }

            if (handController.IsGripPressed()) {
                Rigidbody rb = currentRaycastedtAttractable.GetRigidbody();
                Vector3 direction = (transform.position - rb.position);
                if (direction.sqrMagnitude > squareRadiusToItem) {
                    force = (direction.normalized * forceToItem * Time.fixedDeltaTime) - rb.linearVelocity;
                } else {
                    if (currentRaycastedtAttractable.PerformOnCloseAction(interactionHand)) {
                        ResetCurrentRaycastedAttractable();
                        return true;
                    }
                    direction = direction.normalized * (direction.sqrMagnitude / squareRadiusToItem);
                    force = (direction * forceToItem * Time.fixedDeltaTime) - rb.linearVelocity;
                }
                rb.AddForce(force, ForceMode.VelocityChange);
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, rb.position);
                lineRenderer.enabled = true;
                return true;
            } else {
                lineRenderer.enabled = false;
                return false;
            }
        }

        public bool IsMovingAttractableToHand() {
            return currentRaycastedtAttractable != null && currentRaycastedtAttractable.CanMove() && handController.IsGripPressed();
        }

        public void ResetCurrentRaycastedAttractable() {
            if (currentRaycastedtAttractable != null) {
                if (outlineableAttractable != null) {
                    outlineController.ForceStopOutline(this);
                    outlineableAttractable = null;
                }
                currentRaycastedtAttractable = null;
            }
            lineRenderer.enabled = false;
            force = Vector3.zero;
        }

        protected override void OnInit(HandRaycastData data) {
            if (data != null) {
                initCanRaycast = data.allowRaycast;
                PointFinger = data.pointFinger;
                runtimeCanRaycast = true;
                squareRadiusToItem = radiusToItem * radiusToItem;
            }
            ResetCurrentRaycastedAttractable();
        }

        void OnRaycastUI(HandController controller, RaycastResult result, bool isPressed) {
            if (handController == controller) {
                if (result.isValid) {
                    CreateUIPointer(ref pointer, result.screenPosition, currentGraphicRaycaster.transform, isPressed);
                    controller.SetMagicBarrier(currentGraphicRaycaster.transform.position + (result.worldNormal * maxDistanceToFreeMove), result.worldNormal);
                } else {
                    ResetUIRaycast();
                    controller.StopMagicBarrier();
                }
            }
        }

        void ResetUIRaycast() {
            pointer?.gameObject.SetActive(false);
        }

        void CreateUIPointer(ref HandPointer thisObject, Vector3 screenPosition, Transform parent, bool isPressed = false) {
            if (thisObject == null) {
                thisObject = Instantiate(pointerPrefab);
                thisObject.rectTransform.anchorMin = thisObject.rectTransform.anchorMax = Vector2.zero;
                thisObject.Set(isPressed, true);
            } else {
                thisObject.gameObject.SetActive(true);
                thisObject.Set(isPressed);
            }
            if (thisObject.rectTransform.parent != parent) {
                thisObject.rectTransform.SetParent(parent, false);
                thisObject.rectTransform.SetAsLastSibling();
                thisObject.rectTransform.localScale = Vector3.one;
            }
            thisObject.rectTransform.anchoredPosition3D = screenPosition;
        }

        public class HandRaycastData {
            public Transform pointFinger;
            public bool allowRaycast = true;
        }

    }
}
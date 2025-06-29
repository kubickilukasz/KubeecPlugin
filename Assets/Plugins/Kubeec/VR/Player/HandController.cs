using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kubeec.VR.Player;
using Kubeec.VR.Character;
using System.Threading;

namespace Kubeec.VR.Interactions {
    public class HandController : MonoBehaviour {

        public event Action onBeforeRender;

        [SerializeField] HandRaycaster handRaycaster;
        [SerializeField] CharacterHandTracker handTracker;
        [SerializeField] new Collider collider;
        [SerializeField] float distanceToActivateMagicBarrier = 0.05f;
        [SerializeField] float smoothPosition = 0.05f;
        [SerializeField] float smoothRotation = 0.05f;

        public Camera CurrentCamera => camera;
        public Vector3 TopPointFingerPosition => topPointFinger.position;
        public Vector3 TopPointFingerDirectionFromHand => topPointFinger.position - handPoint.position;
        public Vector3 CurrentHandPosition => handTracker.GetCurrentPosition();
        public bool IsRightHand => isRightHand;
        public bool IsLeftHand => !IsRightHand;
        public bool IsBusy => isBusy;
        public bool IsOwner => isOwner;
        public Rigidbody Rigidbody => handTracker.Rigidbody;

        CharacterHandSetup setup;

        float gripValue;
        float selectValue;
        bool gripDown, selectDown, gripUp, selectUp;
        bool outerOverridedPosAndRot = false;
        bool innerOverridedPosAndRot = false;
        Quaternion inverseOffsetRotation;
        Vector3 signedMultiplierForRotation;
        Vector3? localDirectionFromHandToTopFinger;
        Transform topPointFinger;
        Transform handPoint;
        Vector3 inputHandPosition;
        Quaternion inputHandRotation;
        bool isRightHand;
        bool isBusy;
        bool isOwner;
        new Camera camera;
        Vector3 startOverridedPosition;
        Vector3 characterPositionCurrent; Quaternion characterRotationCurrent;
        Vector3 characterPositionVelocity; Quaternion characterRotationVelocity;

        void FixedUpdate() {
            RaycastAttractable();
        }

        public void Init(CharacterHandSetup setup, Camera camera, bool isRightHand, bool isOwner, ref Action onBeforeRender) {
            this.setup = setup;
            this.camera = camera;
            this.isOwner = isOwner;
            this.isRightHand = isRightHand;
            handRaycaster.Init(new HandRaycaster.HandRaycastData() { pointFinger = setup.GetTopPointFinger(), allowRaycast = isOwner });
            topPointFinger = setup.GetTopPointFinger();
            handPoint = new GameObject("Hand Point").transform;
            handPoint.SetParent(setup.GetWristPoint());
            handPoint.localPosition = Vector3.zero;
            inverseOffsetRotation = Quaternion.Inverse(setup.OffsetRotation);
            signedMultiplierForRotation = new Vector3() {
                z = 1,
                y = 1,
                x = setup.Mutliplier,
            };
            onBeforeRender += this.onBeforeRender;
        }

        public void SetGripAndSelect(float gripValue, float selectValue, bool gripDown, bool selectDown, bool gripUp, bool selectUp) {
            this.gripValue = gripValue;
            this.selectValue = selectValue;
            this.gripDown = gripDown;
            this.selectDown = selectDown;
            this.gripUp = gripUp;
            this.selectUp = selectUp;
        }

        public bool SetMagicBarrier(Vector3 startPositionBarrier, Vector3 directionBarrier) {
            //If someone override position outside the script we have to wait
            if (handTracker.IsPosAndRotOverrided && outerOverridedPosAndRot == true) {
                return false;
            }

            //We need static direction from hand to top finger to calculate position of hand
            if (!innerOverridedPosAndRot) {
                localDirectionFromHandToTopFinger = handTracker.transform.InverseTransformVector(topPointFinger.position - handTracker.GetCurrentPosition());
            }

            Plane barrier = new Plane(directionBarrier, startPositionBarrier);
            Vector3 directionToFingerFromHandInWorldSpace = handTracker.transform.TransformVector(localDirectionFromHandToTopFinger.Value);
            Vector3 worldPositionFinger = GetTargetHandPosition() + directionToFingerFromHandInWorldSpace;
            float distance = barrier.GetDistanceToPoint(worldPositionFinger);
            if (distance < distanceToActivateMagicBarrier) {
                Vector3 position = barrier.ClosestPointOnPlane(worldPositionFinger) - directionToFingerFromHandInWorldSpace;
                handTracker.OverridePositionAndRotation(position, handTracker.GetTargetRotation() * inverseOffsetRotation);
                innerOverridedPosAndRot = true;
                return true;
            } else {
                StopMagicBarrier();
                return false;
            }
        }

        public void StopMagicBarrier() {
            if (innerOverridedPosAndRot) {
                handTracker.StopOverridePositionAndRotation();
                innerOverridedPosAndRot = false;
            }
        }

        public void SetHand(CharacterHandInteraction hand) {
            handTracker.SetHand(hand);
        }

        public bool OverridePositionAndRotation(Vector3 worldPosition, Quaternion worldRotation) {
            startOverridedPosition = worldPosition;
            SmoothPosAndRot(worldPosition, worldRotation);
            handTracker.OverridePositionAndRotation(characterPositionCurrent, characterRotationCurrent);
            outerOverridedPosAndRot = true;
            innerOverridedPosAndRot = false;
            return true;
        }

        public bool StopOverridePositionAndRotation() {
            if (outerOverridedPosAndRot) {
                handTracker.StopOverridePositionAndRotation();
                outerOverridedPosAndRot = false;
                return true;
            }
            return false;
        }

        public void SetPositionAndRotation(Vector3 position, Quaternion rotation) {
            SmoothPosAndRot(position, rotation);
            inputHandPosition = characterPositionCurrent;
            inputHandRotation = characterRotationCurrent;
            handTracker.SetPositionAndRotation(GetTargetHandPosition(inputHandPosition, rotation), inputHandRotation, true);
        }

        public void SetBruteForcePositionAndRotation(Vector3 position, Quaternion rotation) {
            handTracker.SetPositionAndRotation(position, rotation, false);
            inputHandPosition = GetInputHandPosition(position, rotation);
            inputHandRotation = GetInputHandRotation(rotation);
        }

        public void UpdateHandPhysics() {
            handTracker.UpdatePhysics();
        }

        public void UpdateHandTransform(bool wasPhysics, Vector3 offset) {
            handTracker.UpdateTransform(wasPhysics, offset);
        }

        public Vector3 GetCurrentHandPosition() {
            return handTracker.GetCurrentPosition();
        }

        public Quaternion GetCurrentHandRotation() {
            return handTracker.GetCurrentRotation();
        }

        public Vector3 GetTargetHandPosition() {
            return handTracker.GetTargetPosition();
        }

        public Quaternion GetHandTargetRotation() {
            return handTracker.GetTargetRotation();
        }

        public Vector3 GetForceVector() {
            return outerOverridedPosAndRot ? GetTargetHandPosition() - startOverridedPosition : Vector3.zero;
        }

        public Vector3 GetTargetHandPosition(Vector3 inputHandPos, Quaternion inputHandRot) {
            return inputHandPos + inputHandRot * setup.OffsetPosition;
        }

        public Vector3 GetInputHandPosition(Vector3 handPos, Quaternion handRot) {
            return handPos - handRot * setup.OffsetPosition;
        }

        public Quaternion GetInputHandRotation(Quaternion handRot) {
            return handRot * Quaternion.Inverse(setup.OffsetRotation);
        }

        public Vector3 GetInputHandPosition() {
            return inputHandPosition;
        }

        public Quaternion GetInputHandRotation() {
            return inputHandRotation;
        }

        public void GetHandPositionAndRotationPalmByShoulder(Vector3 palmDirection, ref Vector3 position, ref Quaternion rotation) {
            GetHandPositionAndRotationPalm(palmDirection, (transform.position - setup.shoulderReference.position), ref position, ref rotation);
        }

        public void GetHandPositionAndRotationPalm(Vector3 palmDirection, Vector3 baseDirection, ref Vector3 position, ref Quaternion rotation) {
            rotation *= Quaternion.LookRotation(baseDirection, Vector3.up); // rot based on rotation to source
            GetHandPositionAndRotationPalm(palmDirection, ref position, ref rotation);
        }

        public void GetHandPositionAndRotationPalm(Vector3 palmDirection, ref Vector3 realHandPosition, ref Quaternion realHandRotation) {
            realHandRotation *= GetRotationByPalmDirection(palmDirection);
            realHandPosition = GetTargetHandPosition(realHandPosition, realHandRotation);
        }

        public void GetHandPositionAndRotationTopFinger(Vector3 worldTarget, ref Vector3 position, ref Quaternion rotation) {
            Vector3 fromFingerToHand = -TopPointFingerDirectionFromHand;
            Debug.DrawLine(worldTarget, worldTarget + Vector3.up/2, Color.red);
            Debug.DrawRay(worldTarget, fromFingerToHand, Color.blue);
            Debug.DrawLine(topPointFinger.position, handPoint.position, Color.yellow);
            position = position + fromFingerToHand;
            rotation *= inverseOffsetRotation;

            return;
            //We need static direction from hand to top finger to calculate position of hand
            if (!localDirectionFromHandToTopFinger.HasValue) {
                localDirectionFromHandToTopFinger = handTracker.transform.InverseTransformVector(topPointFinger.position - handTracker.GetCurrentPosition());
            }
            Vector3 directionToFingerFromHandInWorldSpace = handTracker.transform.TransformVector(localDirectionFromHandToTopFinger.Value);
            Vector3 worldPositionFinger = GetTargetHandPosition() + directionToFingerFromHandInWorldSpace;
            position = worldTarget + directionToFingerFromHandInWorldSpace;
            //position = worldTarget - directionToFingerFromHandInWorldSpace;
            rotation *= inverseOffsetRotation;
        }

        public void SetBusy(bool isBusy) {
            this.isBusy = isBusy;
        }

        public float GetGripValue() => gripValue;
        public float GetSelectValue() => selectValue;
        public bool IsGripDown() => gripDown;
        public bool IsSelectDown() => selectDown;
        public bool IsGripUp() => gripUp;
        public bool IsSelectUp() => selectUp;
        public bool IsGripPressed() => PlayerControllerBase.IsPressed(GetGripValue());
        public bool IsSelectPressed() => PlayerControllerBase.IsPressed(GetSelectValue());

        public void UpdateFingers() {
            handTracker.SetFingers(gripValue, selectValue);
        }

        void SmoothPosAndRot(Vector3 position, Quaternion rotation) {
            characterPositionCurrent = position;
            characterRotationCurrent = rotation;
        }

        void RaycastAttractable() {
            if (!handTracker.IsPosAndRotOverrided && isOwner && !isBusy) {
                if (!handRaycaster.MoveAttractableToHand() && Time.frameCount % 3 == 0) {
                    handRaycaster.RaycastItem();
                }
            } else {
                handRaycaster.ResetCurrentRaycastedAttractable();
            }
        }

        Quaternion GetRotationByPalmDirection(Vector3 direction) {
            return Quaternion.FromToRotation(Vector3.down, direction.Multiply(signedMultiplierForRotation)) * inverseOffsetRotation;
        }

    }
}

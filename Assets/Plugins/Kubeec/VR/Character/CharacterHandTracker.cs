using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System;

namespace Kubeec.VR.Character {
    public class CharacterHandTracker : MonoBehaviour {

        const float maxDistanceHand = 1.5f;
        const string leftString = "Left"; 
        const string rightString = "Right";
        const float weight = 1f;

        [SerializeField] Hand hand = Hand.Left;
        [SerializeField] CharacterHandInteraction defaultHand;
        [SerializeField] Rigidbody rb;
        [SerializeField] List<Collider> colliders = new();
        [SerializeField] float smoothMove = 0.2f;

        public Hand HandType => hand;

        public bool IsPosAndRotOverrided => isOverridedPosAndRot;
        public Rigidbody Rigidbody => rb;

        protected Quaternion currentOffsetRotation { get; set; }
        protected float currentMultiplier { get; set; }

        CharacterHandInteraction currentHandInteraction;
        CharacterTracker characterTracker;
        CharacterHandSetup handSetup;
        Animator animator;
        AvatarIKGoal goal;

        Vector3 targetPosition;
        Quaternion targetRotation;

        bool isOverridedPosAndRot = false;
        Vector3 overrideTargetPosition;
        Quaternion overrideTargetRotation;

        float gripValue, selectValue;
        int thumbAnimId, indexAnimId, fingersAnimId;
        Vector3 animPositionTarget; Quaternion animRotationTarget;
        Transform helpHand;

        public void UpdateHand() {
            GetCurrentPositionAndRotation(out animPositionTarget, out animRotationTarget);
            if (animator != null) {
                animator.SetIKPositionWeight(goal, weight);
                animator.SetIKRotationWeight(goal, weight);
                animator.SetIKPosition(goal, animPositionTarget);
                animator.SetIKRotation(goal, animRotationTarget);
            }
        }

        public void Init(Animator animator, CharacterTracker characterTracker, CharacterHandSetup setup) {
            this.animator = animator;
            this.characterTracker = characterTracker;
            currentHandInteraction = defaultHand;
            handSetup = setup;
            currentMultiplier = handSetup.Mutliplier;
            currentOffsetRotation = handSetup.OffsetRotation;
            goal = Convert(hand);
            targetPosition = rb.position;
            targetRotation = rb.rotation;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.isKinematic = false;

            thumbAnimId = Animator.StringToHash((hand == Hand.Left ? leftString : rightString) + "Thumb");
            indexAnimId = Animator.StringToHash((hand == Hand.Left ? leftString : rightString) + "Index");
            fingersAnimId = Animator.StringToHash((hand == Hand.Left ? leftString : rightString) + "Fingers");

            helpHand = new GameObject($"HelpHand {hand}").transform;
            helpHand.SetParent(transform.parent);
        }

        public void OverridePositionAndRotation(Vector3 worldPosition, Quaternion worldRotation) {
            isOverridedPosAndRot = true;
            overrideTargetPosition = worldPosition;
            overrideTargetRotation = worldRotation * currentOffsetRotation;
        }

        public void StopOverridePositionAndRotation() {
            isOverridedPosAndRot = false;
        }

        public void SetPositionAndRotation(Vector3 worldPosition, Quaternion worldRotation, bool withOffset = true) {
            targetPosition = worldPosition;
            if (withOffset) {
                worldRotation *= currentOffsetRotation;
            }
            targetRotation = worldRotation;
        }

        public void UpdatePhysics() {
            foreach (Collider c in colliders) {
                c.isTrigger = isOverridedPosAndRot;
            }
            if (maxDistanceHand * maxDistanceHand < (targetPosition - rb.position).sqrMagnitude) {
                rb.isKinematic = true;
                rb.position = targetPosition;
                rb.isKinematic = false;
            } else {
                rb.TryMoveToSmooth(targetPosition, targetRotation, smoothMove);
            }
            //rb.position = rb.transform.position = targetPosition;
            //rb.rotation = rb.transform.rotation = targetRotation;
        }

        public void UpdateTransform(bool wasPhysics, Vector3 offset) {
            helpHand.transform.position = rb.position;
            helpHand.transform.rotation = rb.rotation;
            return;
            if (wasPhysics) {
                helpHand.transform.position = rb.position;
                helpHand.transform.rotation = rb.rotation;
            } else {
                helpHand.transform.position = targetPosition;
                helpHand.transform.rotation = targetRotation;
            }
        }

        public void SetHand(CharacterHandInteraction hand) {
            currentHandInteraction = hand;
            if (currentHandInteraction == null) {
                currentHandInteraction = defaultHand;
            }
            SetFingers(gripValue, selectValue);
        }

        public void SetFingers(float gripValue, float selectValue) {
            this.selectValue = selectValue;
            this.gripValue = gripValue;

            if (currentHandInteraction == null) {
                return;
            }

            float valueFingers = currentHandInteraction.GetFingersState(gripValue, selectValue);
            float valuePointer = currentHandInteraction.GetPointerState(gripValue, selectValue);
            float valueThumb = currentHandInteraction.GetThumbState(gripValue, selectValue);

            animator.SetFloat(fingersAnimId, valueFingers);
            animator.SetFloat(indexAnimId, valuePointer);
            animator.SetFloat(thumbAnimId, valueThumb);
        }

        public Vector3 GetTargetPosition() {
            return targetPosition;
        }

        public Quaternion GetTargetRotation() {
            return targetRotation;
        }

        public Vector3 GetCurrentPosition() {
            //return isOverridedPosAndRot ? overrideTargetPosition : targetPosition;
            return isOverridedPosAndRot ? overrideTargetPosition : transform.position;
        }

        public Quaternion GetCurrentRotation() {
            //return isOverridedPosAndRot ? overrideTargetRotation : targetRotation;
            return isOverridedPosAndRot ? overrideTargetRotation : transform.rotation;
        }

        public void GetCurrentPositionAndRotation(out Vector3 pos, out Quaternion rot) {
            pos = GetCurrentPosition();
            rot = GetCurrentRotation();
        }

        public Vector3 GetCurrentAnimationPosition() {
            return animPositionTarget;
        }

        public Quaternion GetCurrentAnimationRotation() {
            return animRotationTarget;
        }

        AvatarIKGoal Convert(Hand hand) {
            return (AvatarIKGoal)(int)hand;
        }

        public enum Hand {
            Left = 2,
            Right = 3
        }

    }
}

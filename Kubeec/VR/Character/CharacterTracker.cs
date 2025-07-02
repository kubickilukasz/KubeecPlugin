using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;
using Kubeec.VR.Player;

namespace Kubeec.VR.Character {
    public class CharacterTracker : MonoBehaviour, ICharacterAnimatorEventSender {

        const string layerName = "VRController";
        const bool defaultUpdate = true;

        [SerializeField] CharacterHandTracker leftHand;
        [SerializeField] CharacterHandTracker rightHand;
        [SerializeField] CharacterHeadTracker head;
        [SerializeField] CharacterLegTracker leftLeg;
        [SerializeField] CharacterLegTracker rightLeg;

        [Space]

        [SerializeField] CharacterAnimatorReference animatorReference;
        [SerializeField] CharacterSetup characterSetup;

        [Space]

        [SerializeField] float maxHeadAngleToMoveBody = 20f;

        public CharacterSetup CharacterSetup => characterSetup;

        public CharacterHeadTracker HeadTracker => head;

        Action onAnimatorUpdate;
        PlayerController playerController;
        int layerVRController;

        void Reset() {
            animatorReference = GetComponentInChildren<CharacterAnimatorReference>();
        }

        void OnDestroy() {
            if (animatorReference != null) {
                animatorReference.Unregister(this);
            }
        }

        public void UpdateShoulders() {
            characterSetup.UpdateLeftShoulder(leftHand.GetCurrentAnimationPosition());
            characterSetup.UpdateRightShoulder(rightHand.GetCurrentAnimationPosition());
        }

        public void Init(PlayerController playerController, Action onAnimatorUpdate) {
            this.playerController = playerController;
            layerVRController = LayerMask.NameToLayer(layerName);
            animatorReference.animator.enabled = defaultUpdate; // to prevent the animator from updating automatically
            animatorReference.Register(this);
            animatorReference.animator.keepAnimatorStateOnDisable = true;
            characterSetup.Prepare();
            leftHand.Init(animatorReference.animator, this, characterSetup.LeftHandSetup);
            rightHand.Init(animatorReference.animator, this, characterSetup.RightHandSetup);
            head.Init(animatorReference.animator);
            leftLeg.Init(animatorReference.animator, AvatarIKGoal.LeftFoot);
            rightLeg.Init(animatorReference.animator, AvatarIKGoal.RightFoot);
            this.onAnimatorUpdate = onAnimatorUpdate;
            SetRagdoll(false);
        }

        public void SetRagdoll(bool value) {
            characterSetup.RagdollSetup.Set(value, value);
            animatorReference.animator.enabled = value ? false : defaultUpdate;
            if (playerController.IsOwner && characterSetup.HeadRenderer != null) {
                characterSetup.HeadRenderer.gameObject.layer = value ? characterSetup.Head.gameObject.layer : layerVRController;
            }
        }

        public void ForceUpdate() {
            if (!defaultUpdate) {
                animatorReference.animator.enabled = false;
                animatorReference.animator.Update(Time.deltaTime);
            }
        }

        public void OnAnimatorMoveOther() {
            onAnimatorUpdate?.Invoke();
        }

        public void OnAnimatorIKOther(int layerIndex) {
            //We need only for one layer
            if (layerIndex == 0) {
                UpdateHands();
                head.UpdateHead();
                UpdateLegs();
                animatorReference.animator.enabled = defaultUpdate;
            }
        }

        public void UpdateHands() {
            leftHand.UpdateHand();
            rightHand.UpdateHand();
        }

        public void UpdateLegs() {
            bool canMove = leftLeg.IsOnGround && rightLeg.IsOnGround;
            leftLeg.UpdateLeg(animatorReference.transform.position, animatorReference.transform.rotation, playerController.Velocity,
                 leftLeg.transform.position.y - transform.position.y, ref canMove);
            rightLeg.UpdateLeg(animatorReference.transform.position, animatorReference.transform.rotation, playerController.Velocity,
                rightLeg.transform.position.y - transform.position.y, ref canMove);
        }

        public void SetHeadPosition(Vector3 position) {
            animatorReference.transform.position = position - animatorReference.transform.rotation * characterSetup.headLocalPosition;
            head.SetPosition(position);
        }

        public void SetHeadRotation(Quaternion rotation) {
            head.SetRotation(rotation);
            float yAngle = animatorReference.transform.rotation.eulerAngles.y;
            float diff = Mathf.DeltaAngle(yAngle, rotation.eulerAngles.y);
            float adiff = Mathf.Abs(diff);
            if (adiff > maxHeadAngleToMoveBody) {
                float offset = (adiff - maxHeadAngleToMoveBody) * (diff / adiff);
                animatorReference.transform.rotation = Quaternion.Euler(0, yAngle + offset, 0);
            }
        }

    }
}

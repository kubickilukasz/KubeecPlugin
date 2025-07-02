using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kubeec.VR.Outline;
using Kubeec.VR.Player;
using Unity.Netcode;
using System.Linq;


namespace Kubeec.VR.Interactions {

    public class HandInteractor : OrderBasedInteractor<InteractionAttractedHand> {

        [SerializeField] HandController handController;
        [SerializeField] HandRaycaster handRaycaster;
        [SerializeField] OutlineController outlineController;

        public HandRaycaster raycaster => handRaycaster;
        public HandController controller => handController;
        public bool IsOverrided => currentOverridedPosAndRot != null;

        InteractionBase currentOverridedPosAndRot = null;

        protected override void Update() {
            base.Update();
            CheckAttractHands();
            OutlineClosestInteraction();
            HandleRaycastHand();
        }

        public bool CanOverridePositionAndRotation(InteractionBase interaction) {
            return !IsOverrided || currentOverridedPosAndRot == interaction;
        }

        public bool TryOverridePositionAndRotation(InteractionBase interaction, Vector3 position, Quaternion rotation) {
            if (CanOverridePositionAndRotation(interaction)) {
                handController.OverridePositionAndRotation(position, rotation);
                currentOverridedPosAndRot = interaction;
                return true;
            }
            return false;
        }

        public void StopOverridePositionAndRotation(InteractionBase interaction) {
            if (currentOverridedPosAndRot == interaction && handController.StopOverridePositionAndRotation()) {
                currentOverridedPosAndRot = null;
            }
        }

        protected override void OnStartInteract(InteractionBase interaction) {
            if (activeAloneInteraction == null) {
                handController.SetHand(interaction.GetCharacterHand(this));
            }
        }

        protected override void OnStopInteract(InteractionBase interaction) {
            InteractionBase newInteraction = activeAloneInteraction ? activeAloneInteraction : activeInteractions.FirstOrDefault();
            if (newInteraction != null) {
                handController.SetHand(newInteraction.GetCharacterHand(this));
            } else {
                handController.SetHand(null);
            }
            base.OnStopInteract(interaction);
        }

        void OutlineClosestInteraction() {
            if (IsOwner && closestPositionData?.outlineable != null && !IsInteract()) {
                outlineController.StartOutline(closestPositionData.Value.outlineable, this);
            } else  {
                StopOutlineInteraction();
            }
        }

        void StopOutlineInteraction() {
            outlineController.ForceStopOutline(this);
        }

        void CheckAttractHands() {
            if (IsServer && closestPositionData?.interaction is InteractionAttractedHand attractedHand && attractedHand.CanAttract(this)) {
                if (attractedHand.TryAttract(this, closestPositionData?.handHandler)) {
                    currentInteraction = attractedHand;
                }
            }
        }

        void HandleRaycastHand() {
            handRaycaster.SetCanRaycast(closestPositionData?.interaction == null);
        }

    }

}

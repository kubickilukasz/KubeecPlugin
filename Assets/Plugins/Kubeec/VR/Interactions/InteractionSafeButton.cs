using Kubeec.VR.Outline;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace Kubeec.VR.Interactions {

    public class InteractionSafeButton : InteractionTrigger, IOutlineable {

        public event Action<bool> onActionBool;

        [SerializeField] InputInteraction safePressed = InputInteraction.Select;
        [SerializeField] List<OutlineObject> outlineObjects;

        Func<HandInteractor, bool> isDown;

        protected override void OnTriggerEnter(Collider other) {
        }

        protected override void OnTriggerStay(Collider other) {
            if (IsSpawned) {
                HandInteractor handler = other.GetComponentInParent<HandInteractor>();
                if (handler != null && isDown.Invoke(handler)) {
                    if (IsInInteraction(handler)) {
                        StopInteract(handler);
                    } else {
                        StartInteract(handler);
                    }
                }
            }
        }

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            safePressed.GetInputs(out isDown, out _, out _);
        }

        public bool CanOutline() => true;

        public IEnumerable<OutlineObject> GetOutlineObjects(Vector3 source) {
            return outlineObjects;
        }

        protected override void OnInactive() {
            onActionBool?.Invoke(false);
        }

        protected override void OnActive() {
            onActionBool?.Invoke(true);
        }

    }
}

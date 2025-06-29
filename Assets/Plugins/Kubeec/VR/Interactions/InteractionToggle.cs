using UnityEngine;

namespace Kubeec.VR.Interactions {

    [RequireComponent(typeof(Interactor))]
    public class InteractionToggle : InteractionBase {

        [SerializeField] InteractionBase target;
        [SerializeField] InteractionBase source;
        [SerializeField] Interactor handler;

        void Reset() {
            handler = GetComponent<Interactor>();
        }

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            source.onActive += OnSourceActive;
        }

        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();
            if (source) {
                source.onActive -= OnSourceActive;
            }
            StopInteract(handler);
            if (target && target.IsInInteraction(handler)) {
                target.StopInteract(handler);
            }
        }

        void OnSourceActive() {
            if (IsInInteraction(handler)) {
                StopInteract(handler);
                target?.StopInteract(handler);
            } else {
                StartInteract(handler);
                target?.StartInteract(handler);
            }
        }

    }
}

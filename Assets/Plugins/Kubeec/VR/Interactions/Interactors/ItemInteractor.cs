using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kubeec.VR.Interactions {

    [RequireComponent(typeof(InteractionItem))]
    public class ItemInteractor : OrderBasedInteractor<InteractionSocket> {

        public event Action<InteractionSocket> onSocketIn;
        public event Action<InteractionSocket> onSocketOut;

        InteractionItem interactionItem;

        public InteractionItem InteractionItem {
            get {
                if (interactionItem == null) {
                    interactionItem = GetComponent<InteractionItem>();
                }
                return interactionItem;
            }
        }

        protected override void Update() {
            base.Update();
            if (currentInteraction) {
                TrySocketOut();
            } else {
                TrySocketIn();
            }
        }

        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();
            TrySocketOut();
        }

        void TrySocketIn() {
            if (closestPositionData?.interaction is InteractionSocket socket && socket.IsSocketValid(this)) {
                if (socket.CanAttractItem(this) && socket.StartInteract(this)) {
                    currentInteraction = socket;
                    onSocketIn?.Invoke(currentInteraction);
                } else {
                    socket.SetClosestItem(this);
                }
            }
        }

        void TrySocketOut() {
            if (currentInteraction is InteractionSocket socket) {
                if (!socket.CanAttractItem(this)) {
                    socket.StopInteract(this);
                    onSocketOut?.Invoke(socket);
                }
            }
        }

    }

}

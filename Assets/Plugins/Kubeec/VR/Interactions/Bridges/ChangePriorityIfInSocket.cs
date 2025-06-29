using UnityEngine;

namespace Kubeec.VR.Interactions {

    [RequireComponent(typeof(InteractionItem))]
    public class ChangePriorityIfInSocket : EnableDisableInitableDisposable {

        [SerializeField] int priorityChange = 0;

        InteractionItem item;
        ItemInteractor interactor;
        bool wasChanged = false;

        protected override void OnInit(object data) {
            item = GetComponent<InteractionItem>();
            interactor = item.Interactior as ItemInteractor;
            if (interactor) {
                interactor.onSocketIn += OnSocketIn;
                interactor.onSocketOut += OnSocketOut;
            }
        }

        protected override void OnDispose() {
            if (interactor) {
                if (wasChanged) {
                    ChangePriority(-priorityChange);
                }
                interactor.onSocketIn -= OnSocketIn;
                interactor.onSocketOut -= OnSocketOut;
            }
        }

        void OnSocketIn(InteractionSocket interactionSocket) {
            wasChanged = true;
            ChangePriority(priorityChange);
        }

        void OnSocketOut(InteractionSocket interactionSocket) {
            wasChanged = false;
            ChangePriority(-priorityChange);
        }

        void ChangePriority(int value) {
            item.Priority = item.Priority + value;
        }

    }
}

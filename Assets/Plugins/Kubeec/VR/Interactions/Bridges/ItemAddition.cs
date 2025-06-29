using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kubeec.VR.Interactions {

    public abstract class ItemAddition : EnableDisableInitableDisposable {

        [SerializeField] protected InteractionItem item;
        [SerializeField] protected InputInteraction inputInteraction;
        [SerializeField] protected bool controlActiveIfInSocket = false;

        protected ItemInteractor interactor;
        protected InteractionBase[] interactions;

        protected Func<HandInteractor, bool> isDown;
        protected Func<HandInteractor, bool> isPressed;
        protected Func<HandInteractor, float> getValue;
        protected List<HandInteractor> hands = new List<HandInteractor>();

        public InteractionItem Item => item;

        protected override void OnInit(object data) {
            interactor = item.GetComponent<ItemInteractor>();
            interactor.onSocketIn += OnSocketIn;
            interactor.onSocketOut += OnSocketOut;
            inputInteraction.GetInputs(out isDown, out isPressed, out getValue);
            item.onStartInteract.AddListener(RefreshHands);
            item.onStopInteract.AddListener(RefreshHands);
            interactions = GetComponentsInChildren<InteractionBase>(true).Where(x => x != item).ToArray();
            SetInteractionsCanInteract(item.socket == null);
        }

        protected override void OnDispose() {
            interactor.onSocketIn -= OnSocketIn;
            interactor.onSocketOut -= OnSocketOut;
            item.onStartInteract.RemoveListener(RefreshHands);
            item.onStopInteract.RemoveListener(RefreshHands);
        }

        protected virtual void RefreshHands() {
            hands.Clear();
            foreach (Interactor handler in item.GetHandlers()) {
                if (handler is HandInteractor hand) {
                    hands.Add(hand);
                }
            }
        }

        protected virtual void OnSocketIn(InteractionSocket socket) {
            SetInteractionsCanInteract(false);
        }

        protected virtual void OnSocketOut(InteractionSocket socket) {
            SetInteractionsCanInteract(true);
        }

        protected void SetInteractionsCanInteract(bool value) {
            if (controlActiveIfInSocket) {
                foreach (InteractionBase interaction in interactions) {
                    interaction.CanInteract = value;
                }
            }
        }

    }
}

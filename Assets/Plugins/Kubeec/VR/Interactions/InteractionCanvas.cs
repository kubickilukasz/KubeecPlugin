using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using Kubeec.VR.Player;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kubeec.VR.Interactions {

    public class InteractionCanvas : InteractionHandTrigger {
        [InfoBox("Works only for Owner")]
        [SerializeField] HandGraphicRaycaster raycaster;

        public override bool Repeat => true;

        protected virtual void Reset() {
            raycaster = GetComponent<HandGraphicRaycaster>();
        }

        protected override void OnActive() {
            if (raycaster && !raycaster.enabled) raycaster.enabled = true;
            InteractWithCanvas();
        }

        protected override void OnInactive() {
            if (raycaster && raycaster.enabled) raycaster.enabled = false;
        }

        protected override void OnStopInteract(Interactor handler) {
            base.OnStopInteract(handler);
            HandInteractor hand = handler as HandInteractor;
            if (hand.IsOwner) {
                hand.raycaster.StopUIRaycast(raycaster);
            }
        }

        void InteractWithCanvas() {
            foreach (HandInteractor handler in GetHandlers()) {
                if (handler.IsOwner) {
                    handler.raycaster.StartUIRaycast(raycaster);
                }
            }
        }

    }
}
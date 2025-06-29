using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kubeec.VR.Interactions {

    public class InteractionAttractTriggerHand : InteractionAttractedHand {

        [SerializeField] HandHandler triggerHandHandler;

        protected override void OnStopInteract(Interactor handler) {
            (handler as HandInteractor).StopOverridePositionAndRotation(this);
        }

        [NonSerialized] HandHandlerData handHandlerData;
        protected override void OnActive() {
            foreach (HandInteractor handler in GetHandlers()) {
                if (handler.IsOwner) {
                    if (handler.IsClosestInteraction(this)) {
                        HandHandler snapped = reservedPlacesToHandle[handler];
                        handHandlerData = snapped.Get(transform, handler);
                        handler.TryOverridePositionAndRotation(this, handHandlerData.handPosition, handHandlerData.handRotation);
                    } else {
                        handler.StopOverridePositionAndRotation(this);
                    }
                }
            } 
        }

        protected override void OnUpdate() {
            base.OnUpdate();
            foreach (HandInteractor handler in potentialHandlers) {
                if (!handler.IsOwner || IsInInteraction(handler)) {
                    continue;
                }
                HandHandlerData handHandlerData = triggerHandHandler.Get(transform, handler);
                handler.TryOverridePositionAndRotation(this, handHandlerData.handPosition, handHandlerData.handRotation);
            }
        }

        protected override void OnDetract(HandInteractor handler) {
            base.OnDetract(handler);
            handler.StopOverridePositionAndRotation(this);
        }

        protected override void OnEndCheckAttract(HandInteractor handler) {
            base.OnEndCheckAttract(handler);
            handler.StopOverridePositionAndRotation(this);
        }

    }

}

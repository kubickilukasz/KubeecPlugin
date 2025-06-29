using System.Collections.Generic;
using UnityEngine;

namespace Kubeec.VR.Interactions {

    public abstract class TriggerItemAddition : ItemAddition {

        [SerializeField] protected Transform triggerTransform;
        [SerializeField] protected Transform triggerStart;
        [SerializeField] protected Transform triggerEnd;

        [SerializeField] List<HandHandler> handHandlers = new();

        protected float currentForce = 0f;

        protected virtual void Update() {
            if (!IsInitialized()) {
                return;
            }
            foreach (HandInteractor hand in hands) {
                if (CanTrigger(hand)) {
                    UpdateForce(getValue.Invoke(hand));
                }
            }
        }

        protected override void OnInit(object data) {
            base.OnInit(data);
            UpdateForce(0f);
        }

        protected bool CanTrigger(HandInteractor hand) {
            return item.TryGetHandHandler(hand, out HandHandler handHandler) && handHandlers.Contains(handHandler);
        }

        protected override void RefreshHands() {
            UpdateForce(0f);
            base.RefreshHands();
        }

        protected virtual void UpdateForce(float value) {
            triggerTransform.position = Vector3.Lerp(triggerStart.position, triggerEnd.position, value);
            triggerTransform.rotation = Quaternion.Lerp(triggerStart.rotation, triggerEnd.rotation, value);
            currentForce = ModifyValue(value);
        }

        protected virtual float ModifyValue(float value) {
            return value;
        }

    }

}

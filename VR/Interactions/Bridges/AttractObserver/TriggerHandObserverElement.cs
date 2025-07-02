using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Kubeec.VR.Interactions {

    [RequireComponent(typeof(Collider))]
    public class TriggerHandObserverElement : HandObserverElement {

        bool wasFixedUpdate = false;
        HashSet<HandInteractor> oldFrameItems = new HashSet<HandInteractor>();
        HashSet<HandInteractor> itemsInFrame = new HashSet<HandInteractor>();

        void OnTriggerStay(Collider other) {
            if (other.TryGetComponentFromSource(out HandInteractor interactor)) {
                itemsInFrame.Add(interactor);
                CallStay(interactor);
            }
        }

        public override void CallOnUpdate() {
            if (wasFixedUpdate) {
                foreach (HandInteractor item in oldFrameItems) {
                    if (!itemsInFrame.Contains(item)) {
                        CallExit(item);
                    }
                }
                foreach (HandInteractor item in itemsInFrame) {
                    if (!oldFrameItems.Contains(item)) {
                        CallEnter(item);
                    }
                }

                HashSetPool<HandInteractor>.Release(oldFrameItems);
                oldFrameItems = itemsInFrame;
                itemsInFrame = HashSetPool<HandInteractor>.Get();
            }
            wasFixedUpdate = false;
        }

        public override void CallOnFixedUpdate() {
            wasFixedUpdate = true;
        }
    }
}

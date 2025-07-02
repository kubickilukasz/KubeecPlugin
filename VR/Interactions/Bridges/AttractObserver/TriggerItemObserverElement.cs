using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Kubeec.VR.Interactions {

    public class TriggerItemObserverElement : ItemObserverElement {

        bool wasFixedUpdate = false;
        HashSet<ItemInteractor> oldFrameItems = new HashSet<ItemInteractor>();
        HashSet<ItemInteractor> itemsInFrame = new HashSet<ItemInteractor>();

        void OnTriggerStay(Collider other) {
            if (other.TryGetComponentFromSource(out ItemInteractor interactor)) {
                itemsInFrame.Add(interactor);
                CallStay(interactor);
            }
        }

        public override void CallOnUpdate() {
            if (wasFixedUpdate) {
                foreach (ItemInteractor item in oldFrameItems) {
                    if (!itemsInFrame.Contains(item)) {
                        CallExit(item);
                    }
                }
                foreach (ItemInteractor item in itemsInFrame) {
                    if (!oldFrameItems.Contains(item)) {
                        CallEnter(item);
                    }
                }

                HashSetPool<ItemInteractor>.Release(oldFrameItems);
                oldFrameItems = itemsInFrame;
                itemsInFrame = HashSetPool<ItemInteractor>.Get();
            }
            wasFixedUpdate = false;
        }

        public override void CallOnFixedUpdate() {
            wasFixedUpdate = true;
        }

    }
}

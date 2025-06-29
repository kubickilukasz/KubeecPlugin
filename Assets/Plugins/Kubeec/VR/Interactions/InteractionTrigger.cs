using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using System.Linq;

namespace Kubeec.VR.Interactions {

    public class InteractionTrigger : InteractionTriggerBase<Interactor> {
    }

    public class InteractionTriggerBase<T> : InteractionBase where T : Interactor {

        public event Action<T> onTriggerEnter;
        public event Action<T> onTriggerExit;

        protected virtual void OnTriggerEnter(Collider other) {
            if (other.TryGetComponentFromSource(out T handler)) {
                StartInteract(handler);
            }
        }

        protected virtual void OnTriggerStay(Collider other) {
        }

        protected virtual void OnTriggerExit(Collider other) {
            if (other.TryGetComponentFromSource(out T handler) ) {
                StopInteract(handler);
            }
        }

        protected override void OnStartInteract(Interactor handler) {
            onTriggerEnter?.Invoke(handler as T);
        }

        protected override void OnStopInteract(Interactor handler) {
            onTriggerExit?.Invoke(handler as T);
        }

    }

}

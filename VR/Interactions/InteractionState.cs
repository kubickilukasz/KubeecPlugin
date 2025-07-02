using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Kubeec.VR.Interactions {

    public abstract class InteractionState<T> : InteractionAttractedHand, IStateVariable<T> {
        public event Action<T> onChangeState;

        public T GetState() {
            throw new NotImplementedException();
        }

        public void SetState(T value) {
            throw new NotImplementedException();
        }
    }

}
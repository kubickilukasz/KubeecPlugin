using UnityEngine;
using System;

namespace Kubeec.VR.Interactions {

    public abstract class ObserverElement<T, Y> : MonoBehaviour where T : Interactor where Y : ObserverElement<T, Y>{

        public event Action<Y, T> onEnter;
        public event Action<Y, T> onStay;
        public event Action<Y, T> onExit;

        public abstract void CallOnUpdate();
        public abstract void CallOnFixedUpdate();

        protected void CallEnter(T interactor) {
            onEnter?.Invoke(this as Y, interactor);
        }

        protected void CallStay(T interactor) {
            onStay?.Invoke(this as Y, interactor);
        }

        protected void CallExit(T interactor) {
            onExit?.Invoke(this as Y, interactor);
        }

    }

}

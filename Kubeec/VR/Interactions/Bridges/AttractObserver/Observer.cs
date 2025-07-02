using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kubeec.VR.Interactions {

    public abstract class Observer<T, Y> : MonoBehaviour where T : Interactor where Y : ObserverElement<T, Y> {

        [SerializeField] List<Y> observers = new();

        Registered registeredEnter = new();
        Registered registeredStay = new();
        Registered registeredExit = new();

        [Button]
        void Reset() {
            observers = new(GetComponentsInChildren<Y>());
        }

        void OnEnable() {
            foreach (var item in observers) {
                if (item != null) {
                    item.onEnter += HandleEnter;
                    item.onStay += HandleStay;
                    item.onExit += HandleExit;
                }
            }
        }

        void OnDisable() {
            foreach (var item in observers) {
                if (item != null) {
                    item.onEnter -= HandleEnter;
                    item.onStay -= HandleStay;
                    item.onExit -= HandleExit;
                }
            }
        }

        void FixedUpdate() {
            foreach (Y observer in observers) {
                observer.CallOnFixedUpdate();
            }
        }

        void Update() {
            foreach (Y observer in observers) {
                observer.CallOnUpdate();
            }
        }

        public void RegisterEnter(Action<T, GameObject> callback, MonoBehaviour behaviour) {
            Register(behaviour, callback, ref registeredEnter);
        }

        public void RegisterStay(Action<T, GameObject> callback, MonoBehaviour behaviour) {
            Register(behaviour, callback, ref registeredStay);
        }

        public void RegisterExit(Action<T, GameObject> callback, MonoBehaviour behaviour) {
            Register(behaviour, callback, ref registeredExit);
        }

        public void UnregisterEnter(MonoBehaviour behaviour) {
            Unregister(behaviour, ref registeredEnter);
        }

        public void UnregisterStay(MonoBehaviour behaviour) {
            Unregister(behaviour, ref registeredStay);
        }

        public void UnregisterExit(MonoBehaviour behaviour) {
            Unregister(behaviour, ref registeredExit);
        }

        void Register(MonoBehaviour behaviour, Action<T, GameObject> callback, ref Registered registered) {
            registered.Add(behaviour, new ObserverHandler(callback));
        }

        void Unregister(MonoBehaviour behaviour, ref Registered registered) {
            registered.Remove(behaviour);
        }

        void HandleEnter(Y observer, T handler) {
            HandleEvent(observer, handler, ref registeredEnter);
        }

        void HandleStay(Y observer, T handler) {
            HandleEvent(observer, handler, ref registeredStay);
        }

        void HandleExit(Y observer, T handler) {
            HandleEvent(observer, handler, ref registeredExit);
        }

        void HandleEvent(Y observer, T handler, ref Registered registered) {
            foreach (KeyValuePair<MonoBehaviour, ObserverHandler> item in registered) {
                item.Value.callback.Invoke(handler, observer.gameObject);
            }
        }

        internal class Registered : Dictionary<MonoBehaviour, ObserverHandler> { }

        internal class ObserverHandler {
            public Action<T, GameObject> callback;

            public ObserverHandler(Action<T, GameObject> callback) {
                this.callback = callback;
            }

        }

    }

}

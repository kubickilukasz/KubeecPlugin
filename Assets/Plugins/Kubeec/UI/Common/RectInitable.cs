using System;
using UnityEngine;

namespace UI {

    public abstract class RectInitable<T> : RectMonoBehaviour, IInitable<T> where T : class {

        bool wasInit = false;

        public void Init(T data = null) {
            if (wasInit) return;
            wasInit = true;
            OnInit(data);
        }

        public bool IsInitialized() => wasInit;

        protected virtual void OnInit(T data) {

        }

    }

    public abstract class RectInitableDisposable<T> : RectMonoBehaviour, IInitable<T>, IDisposable where T : class {

        public event Action onInit;
        public event Action onDispose;

        bool wasInit = false;

        public void Init(T data = null) {
            if (wasInit) return;
            wasInit = true;
            OnInit(data);
            onInit?.Invoke();
        }

        public bool IsInitialized() => wasInit;

        public void Dispose() {
            if (!wasInit) return;
            wasInit = false;
            OnDispose();
            onDispose?.Invoke();
        }

        protected virtual void OnInit(T data) {

        }

        protected virtual void OnDispose() {

        }

    }

    public abstract class RectInitable : RectInitable<object> {
    }

    public abstract class RectInitableDisposable : RectInitableDisposable<object> {
    }

    public abstract class StartRectInitable : RectInitable {

        void Start() {
            Init();
        }

    }

    public abstract class EnableRectInitableDisposable : RectInitableDisposable {

        void OnEnable() {
            Init();
        }

    }

    public abstract class DisableRectInitableDisposable : RectInitableDisposable {

        void OnDisable() {
            Dispose();
        }

    }

    public abstract class EnableDisableRectInitableDisposable : RectInitableDisposable {

        void OnEnable() {
            Init();
        }

        void OnDisable() {
            Dispose();
        }


    }

    public abstract class EnableDisableRectInitableDisposable<T> : RectInitableDisposable<T> where T : class {

        void OnEnable() {
            Init();
        }

        void OnDisable() {
            Dispose();
        }

    }


}

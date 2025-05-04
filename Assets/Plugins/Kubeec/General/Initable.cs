using System;
using UnityEngine;

public interface IInitable<T> where T : class{

    public void Init(T data = null);

    public bool IsInitialized();

}

public abstract class Initable<T> : MonoBehaviour, IInitable<T> where T : class {

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

public abstract class InitableDisposable<T> : MonoBehaviour, IInitable<T>, IDisposable where T : class {

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

public abstract class Initable : Initable<object> {
}

public abstract class InitableDisposable : InitableDisposable<object> {
}

public abstract class StartInitable : Initable {

    void Start() {
        Init();
    }

}

public abstract class EnableInitableDisposable : InitableDisposable {

    void OnEnable() {
        Init();
    }

}

public abstract class DisableInitableDisposable : InitableDisposable{

    void OnDisable() {
        Dispose();
    }

}

public abstract class EnableDisableInitableDisposable : InitableDisposable {

    void OnEnable() {
        Init();
    }

    void OnDisable() {
        Dispose();
    }


}

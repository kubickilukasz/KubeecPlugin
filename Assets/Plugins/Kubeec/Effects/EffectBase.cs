using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EffectBase : EnableInitableDisposable{

    public bool playOnSpawn = true;
    public bool destroyOnEnd = true;
    public float duration = 0f;

    public EffectBase prefabReference { set; get; } = null;

    Coroutine coroutine;

    [ContextMenu("Play")]
    public void Play() {
        if (!IsInitialized()) {
            return;
        }

        if (coroutine != null) {
            StopCoroutine(coroutine);
        }
        PlayServerRpc();
        if (duration > 0) {
            coroutine = this.InvokeDelay(InternalOnEndPlay, duration);
        }
    }

    public void Stop() {
        if (!IsInitialized()) {
            return;
        }
        if (coroutine != null) {
            StopCoroutine(coroutine);
        }
        OnStop();
    }

    protected override void OnInit(object data) {
        OnStart();
        if (playOnSpawn) {
            Play();
        }
    }

    protected override void OnDispose() {
        if (prefabReference != null) {
            EffectPool.Release(prefabReference, this);
            prefabReference = null;
        } else {
            Destroy(gameObject);
        }
    }

    public virtual bool IsPlaying() => IsInitialized();
    protected abstract void OnStart();
    protected abstract void OnPlay();
    protected virtual void OnEndPlay() { }
    protected virtual void OnStop() { }

    void PlayServerRpc() {
        PlayClientRpc();
    }

    void PlayClientRpc() {
        OnPlay();
    }

    void InternalOnEndPlay() {
        OnEndPlay();
        if (destroyOnEnd) {
            Dispose(); 
        }
    }

}

public static class EffectBaseExt {

    public static EffectBase Create(this EffectBase prefab, Vector3 position, Vector3 normal, Transform parent = null) {
        Quaternion rotation = Quaternion.LookRotation(normal, Vector3.up);
        return prefab.Create(position, rotation, parent);
    }

    public static EffectBase Create(this EffectBase prefab, Vector3 position, Quaternion? rotation = null, Transform parent = null) {
        EffectBase effect = EffectPool.Get(prefab);
        if (parent) {
            effect.transform.SetParent(parent, true);
        }
        if (!rotation.HasValue) {
            rotation = Quaternion.identity;
        }
        effect.prefabReference = prefab;
        effect.gameObject.transform.position = position;
        effect.gameObject.transform.rotation = rotation.Value;
        effect.Init();
        return effect;
    }


    public static EffectBase CreateAndPlay(this EffectBase prefab, Vector3 position, Vector3 normal, Transform parent = null) {
        Quaternion rotation = Quaternion.LookRotation(normal, Vector3.up);
        return prefab.CreateAndPlay(position, rotation, parent);
    }

    public static EffectBase CreateAndPlay(this EffectBase prefab, Vector3 position, Quaternion? rotation = null, Transform parent = null) {
        EffectBase effect = prefab.Create(position, rotation, parent);
        if (!effect.playOnSpawn) {
            effect.Play();
        }
        return effect;
    }

}

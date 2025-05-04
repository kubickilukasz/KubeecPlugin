using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public abstract class EffectBase : NetworkBehaviour{

    [SerializeField] protected bool playOnSpawn = true;
    [SerializeField] protected bool destroyOnEnd = true;
    [SerializeField] protected float duration = 0f;

    public bool PlayOnSpawn => playOnSpawn;
    public float Duration => duration;
    public bool DestroyOnEnd => destroyOnEnd;

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        OnStart();
        if (playOnSpawn) {
            Play();
        }
    }

    [ContextMenu("Play")]
    public void Play() {
        PlayServerRpc();
        if (duration > 0) {
            this.InvokeDelay(InternalOnEndPlay, duration);
        }
    }

    protected abstract void OnStart();
    protected abstract void OnPlay();
    protected virtual void OnEndPlay() { }

    [ServerRpc(RequireOwnership = false)]
    void PlayServerRpc() {
        PlayClientRpc();
    }

    [ClientRpc(RequireOwnership = false)]
    void PlayClientRpc() {
        OnPlay();
    }

    protected void InternalOnEndPlay() {
        OnEndPlay();
        if (destroyOnEnd) {
            if (IsSpawned && IsServer) {
                NetworkObject.Despawn(true);
            } else {
                Destroy(gameObject);
            }
        }
    }

}

public static class EffectBaseExt {

    public static EffectBase CreateAndPlay(this EffectBase prefab, Vector3 position, Quaternion? rotation = null, Transform parent = null) {
        EffectBase effect = Object.Instantiate(prefab);
        if (effect.NetworkObject == null) {
            GameObject.Destroy(effect.gameObject);
            return null;
        }
        if (parent) {
            effect.transform.SetParent(parent, false);
        }
        if (!rotation.HasValue) {
            rotation = Quaternion.identity;
        }
        effect.gameObject.transform.position = position;
        effect.gameObject.transform.rotation = rotation.Value;
        effect.NetworkObject.Spawn();
        if (!effect.PlayOnSpawn) {
            effect.Play();
        }
        return effect;
    }

}

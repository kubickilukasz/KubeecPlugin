using System;
using Unity.Netcode;
using UnityEngine;

public class Destructible : MonoBehaviour{

    public event Action onDestruct;

    [SerializeField] EffectBase effectBase;

    public void Destruct() {
        if (this == null) {
            return;
        }

        if (effectBase) {
            effectBase.CreateAndPlay(transform.position);
        }
        onDestruct?.Invoke();
        Debug.Log($"Destruct {name}");
        if (TryGetComponent(out IDestructible destructible)) {
            Debug.Log($"IDestructible {destructible}");
            destructible.Destruct();
        } else if (TryGetComponent(out NetworkObject networkObject)) {
            Debug.Log($"NetworkObject {networkObject}");
            networkObject.Despawn(true);
        } else {
            Debug.Log($"Destroy {networkObject}");
            Destroy(gameObject);
        }
    }

}

public interface IDestructible {
    public void Destruct();

}

public static class DestructableExt {

    public static bool Destruct(this MonoBehaviour monoBehaviour) {
        if (monoBehaviour.TryGetComponent(out IDestructible destructible)) {
            destructible.Destruct();
            return true;
        } else {
            return false;
        }
    }

}
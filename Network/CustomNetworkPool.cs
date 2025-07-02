using Kubeec.General;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

namespace Kubeec.Network {

    public abstract class CustomNetworkPool<T> : NetworkBehaviour where T : CustomNetworkPool<T>{

        const string nameDynamicHandler = "[Generated] {0}";

        [SerializeField] GameObject prefab;
        ObjectPool<NetworkObject> pool;

        public override void OnNetworkSpawn() {
            if (prefab.TryGetComponent(out NetworkObject netObj)) {
                pool = new ObjectPool<NetworkObject>(OnCreate, OnGet, OnRelease);
                NetworkManager.Singleton.PrefabHandler.AddHandler(prefab, new PooledPrefabInstanceHandler<T>(this));
            } else {
                Debug.LogError($"Prefab without NetworkObject");
            }
        }

        public override void OnNetworkDespawn() {
            NetworkManager.Singleton.PrefabHandler.RemoveHandler(prefab);
            pool.Clear();
        }

        public NetworkObject GetNetworkObject(Vector3 position, Quaternion rotation) {
            NetworkObject networkObject = pool.Get();

            Transform noTransform = networkObject.transform;
            noTransform.position = position;
            noTransform.rotation = rotation;
            if (IsServer && !networkObject.IsSpawned) {
                networkObject.Spawn();
            }

            return networkObject;
        }

        public void ReturnNetworkObject(NetworkObject networkObject) {
            pool.Release(networkObject);
        }

        NetworkObject OnCreate() {
            NetworkObject handler = GameObject.Instantiate(prefab).GetComponent<NetworkObject>();
            handler.name = string.Format(nameDynamicHandler, prefab.name);
            return handler;
        }

        void OnGet(NetworkObject handler) {
            handler.gameObject.SetActive(true);
        }

        void OnRelease(NetworkObject handler) {
            handler.gameObject.SetActive(false);
        }

    }

    class PooledPrefabInstanceHandler<T> : INetworkPrefabInstanceHandler where T : CustomNetworkPool<T>{

        CustomNetworkPool<T> pool;

        public PooledPrefabInstanceHandler(CustomNetworkPool<T> pool) {
            this.pool = pool;
        }

        NetworkObject INetworkPrefabInstanceHandler.Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation) {
            return pool.GetNetworkObject(position, rotation);
        }

        void INetworkPrefabInstanceHandler.Destroy(NetworkObject networkObject) {
            pool.ReturnNetworkObject(networkObject);
        }
    }

}
using Kubeec.NPC;
using System;
using Unity.Netcode;
using UnityEngine;

namespace Kubeec.Network {

    [RequireComponent(typeof(NetworkObject))]
    public class PooledNPC : Initable<GameObject>, IDestructible {

        public event Action<PooledNPC> onDestruct;

        [SerializeField] NonPlayerCharacter nonPlayerCharacter;

        public NonPlayerCharacter NonPlayerCharacter => nonPlayerCharacter;

        GameObject prefab;
        NetworkObjectPool pool;
        NetworkObject networkObject;

        protected override void OnInit(GameObject data) {
            prefab = data;
            pool = NetworkObjectPool.Singleton;
            networkObject = GetComponent<NetworkObject>();
        }

        public void Destruct() {
            onDestruct?.Invoke(this);
            pool.ReturnNetworkObject(networkObject, prefab);
        }

    }

}

using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Kubeec.Network {

    public class EffectSpawner : EffectBase {

        [SerializeField] Bounds bounds;
        [SerializeField] List<NetworkObject> networkObjects = new();

        protected override void OnPlay() {
            Quaternion quaternion = transform.rotation;
            networkObjects.ForEach(x => {
                NetworkObjectPool.Singleton
                .GetNetworkObject(x.gameObject, bounds.GetRandomPosition(transform.position), quaternion)
                .Spawn();
                quaternion = Quaternion.Inverse(quaternion);
            });
        }

        protected override void OnStart() {

        }
    }

}

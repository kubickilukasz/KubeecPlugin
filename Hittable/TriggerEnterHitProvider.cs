using System.Collections.Generic;
using UnityEngine;

namespace Kubeec.Hittable {

    public class TriggerEnterHitProvider : HitProvider {

        public Transform contact;
        [SerializeField] List<TriggerHitDefinition> collidersHitDefinition = new();

        void OnTriggerEnter(Collider collider) {
            if (collider.TryGetComponent(out HitReceiver hitReceiver)) {
                foreach (TriggerHitDefinition colliderHit in collidersHitDefinition) {
                    colliderHit.effectBase?.CreateAndPlay(contact.position, contact.forward);
                    HitInfo hitInfo = CreateHit(hitReceiver, colliderHit.hitType, colliderHit.damage, contact.position, contact.forward);
                    SendHit(hitInfo);
                }
            }
        }

        [System.Serializable]
        public class TriggerHitDefinition {
            public EffectBase effectBase;
            public HitType hitType;
            public float damage = 1f;
        }

    }

}

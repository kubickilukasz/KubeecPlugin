using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kubeec.Hittable {

    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsHitProvider : HitProvider {

        [Space]
        [SerializeField] List<ColliderHitDefinition> collidersHitDefinition = new();

        void OnCollisionEnter(Collision collision) {
            if (collision.collider.TryGetComponent(out HitReceiver hitReceiver)) {
                float force = (collision.impulse / Time.fixedDeltaTime).magnitude;
                ContactPoint contact = collision.GetContact(0);
                foreach (ColliderHitDefinition colliderHit in collidersHitDefinition) {
                    if (colliderHit.collider.Equals(contact.thisCollider) && colliderHit.minForce <= force) {
                        TakeHit(hitReceiver, colliderHit.hitType, force * colliderHit.damagePerForce, contact.point, contact.normal);
                    }
                }
            }
        }

        [System.Serializable]
        public class ColliderHitDefinition {
            public Collider collider;
            public HitType hitType;
            public float minForce = 1f;
            public float damagePerForce = 1f;
        }

    }

}
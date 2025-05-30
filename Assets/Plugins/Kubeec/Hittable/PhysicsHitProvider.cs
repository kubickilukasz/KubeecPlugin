using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Kubeec.Hittable {

    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsHitProvider : HitProvider {

        public UnityEvent onCollisionEnter;

        [SerializeField] List<ColliderHitDefinition> collidersHitDefinition = new();

        void OnCollisionEnter(Collision collision) {
            if (collision.collider.TryGetComponent(out HitReceiver hitReceiver)) {
                float force = (collision.impulse / Time.fixedDeltaTime).magnitude;
                ContactPoint contact = collision.GetContact(0);
                foreach (ColliderHitDefinition colliderHit in collidersHitDefinition) {
                    if (colliderHit.collider.Equals(contact.thisCollider)) {
                        if (colliderHit.minForce <= force) {
                            colliderHit.effectBaseReachMinForce?.CreateAndPlay(contact.point, contact.normal);
                            float damage = colliderHit.useStaticDamage ? colliderHit.staticDamage : force * colliderHit.damagePerForce;
                            HitInfo hitInfo = CreateHit(hitReceiver, colliderHit.hitType, damage, contact.point, contact.normal);
                            SendHit(hitInfo);
                        }
                    }
                }
            }
            onCollisionEnter?.Invoke();
        }

    }

}
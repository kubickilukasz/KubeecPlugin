using System.Collections;
using UnityEngine;

namespace Kubeec.Hittable {

    public class PhysicsHitReceiver : HitReceiver {

        [SerializeField] float minForce = 1f;
        [SerializeField] float damagePerForce = 1f;
        [SerializeField] HitType hitType;

        void OnCollisionEnter(Collision collision) {
            float force = (collision.impulse / Time.fixedDeltaTime).magnitude;
            ContactPoint contact = collision.GetContact(0);
            if (minForce <= force) {
                HitInfo info = HitProvider.CreateHit(this, null, hitType, force * damagePerForce, contact.point, contact.normal);
                TakeHit(info);
            }
        }


    }

}
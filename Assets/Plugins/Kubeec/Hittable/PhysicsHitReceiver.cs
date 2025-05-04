using System.Collections;
using UnityEngine;

namespace Kubeec.Hittable {

    public class PhysicsHitReceiver : HitReceiver {

        [Space]
        [SerializeField] float minForce = 1f;
        [SerializeField] float damagePerForce = 1f;
        [SerializeField] HitType hitType;

        void OnCollisionEnter(Collision collision) {
            float force = (collision.impulse / Time.fixedDeltaTime).magnitude;
            if (minForce <= force) {
                TakeHit(null, hitType, force * damagePerForce);
            }
        }


    }

}
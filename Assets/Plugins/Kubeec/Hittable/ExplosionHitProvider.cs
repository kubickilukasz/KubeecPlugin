using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kubeec.Hittable {
    public class ExplosionHitProvider : HitProvider {

        [SerializeField] Vector3 offsetPosition;
        [SerializeField] float radius;
        [SerializeField] LayerMask layerMask;
        [SerializeField] List<HitReceiver> ignoredReceivers = new();

        [Space]

        [SerializeField] float baseDamage;
        [SerializeField] HitType hitType = HitType.Explosive;
        [Tooltip("1 means baseDamage, others are multiplication of base damage")]
        [SerializeField] AnimationCurve distanceDamageCurve;
        [SerializeField] float baseDelay;
        [Tooltip("0 means no delay, others values means seconds")]
        [SerializeField] AnimationCurve distanceDelayCurve;

        [Space]

        [SerializeField] bool useForceToRigibody = false;
        [SerializeField] float baseForce;
        [SerializeField] AnimationCurve damageForceCurve;

        [Space]

        [SerializeField] bool hitOnEnable = false;

        void OnDrawGizmosSelected() {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.TransformPoint(offsetPosition), radius);
        }

        protected override void OnInit(object data) {
            base.OnInit(data);
            if (hitOnEnable) {
                Explosion();
            }
        }

        public void Explosion() {
            if (!IsInitialized()) {
                return;
            }
            playEffect = false;
            Vector3 center = transform.TransformPoint(offsetPosition);
            effectBase?.CreateAndPlay(center, transform.rotation);
            Collider[] hits = Physics.OverlapSphere(center, radius, layerMask);
            if (hits.Length > 0) {
                for (int i = 0; i < hits.Length; i++) {
                    HandleCollider(hits[i], center);
                }
            }
            Dispose();
        }

        void HandleCollider(Collider collider, Vector3 center) {
            if (collider.TryGetComponent(out HitReceiver hitReceiver) && !ignoredReceivers.Contains(hitReceiver)) {
                float distance = Vector3.Distance(center, hitReceiver.transform.position);
                float tDistance = distance / radius;
                float damage = baseDamage * distanceDamageCurve.Evaluate(tDistance);
                float delay = baseDelay * distanceDelayCurve.Evaluate(tDistance);
                damage = TakeHit(hitReceiver, hitType, damage, center, null, delay);
                if (useForceToRigibody && collider.attachedRigidbody != null && !collider.attachedRigidbody.isKinematic) {
                    Rigidbody rb = collider.attachedRigidbody;
                    hitReceiver.InvokeDelay(() => {
                        float explosionForce = damageForceCurve.Evaluate(damage / baseDamage) * baseForce;
                        rb.AddExplosionForce(explosionForce, center, radius, 0f, ForceMode.Impulse);
                    }, delay);
                }
            }
        }

    }
}
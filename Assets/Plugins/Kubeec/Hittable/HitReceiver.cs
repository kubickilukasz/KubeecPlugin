using System;
using System.Collections;
using UnityEngine;

namespace Kubeec.Hittable {

    public class HitReceiver : MonoBehaviour {

        public event Action<float> onHit;

        [SerializeField] protected float damageMultiplier = 1f;
        [SerializeField] protected float maxDamage = -1;

        HitCollector _collector;
        protected HitCollector collector {
            get {
                if (_collector == null) {
                    _collector = GetComponentInParent<HitCollector>(true);
                }
                return _collector;
            }
        }

        public float TakeHit(HitProvider hitProvider, HitType hitType, float damage) {
            SendHitToCollector(hitProvider, hitType, damage);
            return damage;
        }

        public float TakeHit(HitProvider hitProvider, HitType hitType, float damage, float delay) {
            this.InvokeDelay(() => SendHitToCollector(hitProvider, hitType, damage), delay);
            return damage;
        }

        protected virtual float GetDamage(HitType hitType, float damage) {
            damage *= damageMultiplier;
            if (maxDamage > 0) {
                damage = Mathf.Clamp(damage, 0, maxDamage);
            }
            return damage;
        }

        protected virtual void SendHitToCollector(HitProvider hitProvider, HitType hitType, float damage) {
            if (collector.CollectDamage(this, hitProvider, hitType, GetDamage(hitType, damage))) {
                onHit?.Invoke(damage);
            }
        }

    }

}

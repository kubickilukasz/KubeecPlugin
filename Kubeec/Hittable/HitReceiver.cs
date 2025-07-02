using System;
using System.Collections;
using UnityEngine;

namespace Kubeec.Hittable {

    public class HitReceiver : MonoBehaviour {

        public event Action<float> onHit;

        [SerializeField] protected EffectBase effect;
        [SerializeField] protected float damageMultiplier = 1f;
        [SerializeField] protected float minDamage = -1;
        [SerializeField] protected float maxDamage = -1;
        [SerializeField] protected bool canHeal = true;

        HitCollector _collector;
        protected HitCollector collector {
            get {
                if (_collector == null) {
                    _collector = GetComponentInParent<HitCollector>(true);
                }
                return _collector;
            }
        }

        public float TakeHit(HitInfo hitInfo) {
            hitInfo.damage = GetDamage(hitInfo.type, hitInfo.damage);
            if (hitInfo.delay.HasValue) {
                this.InvokeDelay(() => SendHitToCollector(hitInfo), hitInfo.delay.Value);
            } else {
                SendHitToCollector(hitInfo);
            }
            return hitInfo.damage;
        }

        protected virtual float GetDamage(HitType hitType, float damage) {
            damage *= damageMultiplier;
            float absDamage = Mathf.Abs(damage);
            if (maxDamage > 0 && maxDamage < absDamage) {
                damage = maxDamage * (damage/absDamage);
            }else if (minDamage > 0 && minDamage > damage) {
                damage = 0f;
            }
            if ((!canHeal || hitType != HitType.Heal) && damage < 0f) {
                damage = 0f;
            }
            return damage;
        }

        protected virtual void SendHitToCollector(HitInfo hitInfo) {
            if (collector != null && collector.CollectDamage(hitInfo)) {
                OnHit(hitInfo);
                onHit?.Invoke(hitInfo.damage);
            }
        }

        protected virtual void OnHit(HitInfo hitInfo) {
            effect?.CreateAndPlay(hitInfo.position ?? transform.position,
                    hitInfo.normal.HasValue ? Quaternion.LookRotation(hitInfo.normal.Value) : transform.rotation);
        }

    }

}

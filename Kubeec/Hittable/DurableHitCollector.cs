using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kubeec.Hittable {

    public class DurableHitCollector : HitCollector {

        [SerializeField] [Range(0,1f)] float percentDamageChange = 0.05f;
        [SerializeField] float maxDistanceToWound = 0.1f;
        [SerializeField] DurableEffect durableEffect;

        List<DurableEffect> existedWounds = new();

        public void ClearAllWounds() {
            existedWounds.ForEach(w => w.Dispose());
            existedWounds.Clear();
        }

        protected override void OnHit(HitInfo hitInfo) {
            float t = 1f - currentHealth / maxHealth;
            float howManyWounds = t / percentDamageChange;
            float totalWoundsValue = existedWounds.Select(w => w.deepValue).Sum();
            if (howManyWounds == totalWoundsValue) {
                return;
            }
            float diff = Mathf.Abs(howManyWounds - totalWoundsValue);
            Vector3 hitPosition = hitInfo.position ?? hitInfo.hitReceiver.transform.position;
            DurableEffect effect = FindWound(hitPosition);
            if (howManyWounds > totalWoundsValue && hitInfo.damage > 0) {
                if (effect == null) {
                    effect = durableEffect.Create(hitPosition, hitInfo.normal ?? Vector3.zero, transform) as DurableEffect;
                    effect.SetDeep(diff);
                    existedWounds.Add(effect);
                    effect.Play();
                } else {
                    effect.SetDeep(effect.deepValue + diff);
                    effect.Play();
                }
            } else if(existedWounds.Count > 0 && hitInfo.damage < 0) {
                if (effect == null) {
                    effect = existedWounds[0];
                }
                float newDeep = effect.deepValue - diff;
                if (newDeep > 0f) {
                    effect.SetDeep(newDeep);
                } else {
                    existedWounds.Remove(effect);
                    effect.Dispose();
                }
            }
        }

        protected override void OnDispose() {
            base.OnDispose();
            //ClearAllWounds();
        }

        DurableEffect FindWound(Vector3 position) {
            DurableEffect current = null;
            float currentDistance = float.MaxValue;
            foreach (DurableEffect w in existedWounds) {
                float distance = Vector3.Distance(position, w.transform.position);
                if (distance < maxDistanceToWound && distance < currentDistance) {
                    currentDistance = distance;
                    current = w;
                }
            }
            return current;
        }

    }

}

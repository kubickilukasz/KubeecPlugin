using System.Collections;
using UnityEngine;

namespace Kubeec.Hittable {

    public class HitProvider : EnableDisableInitableDisposable {

        [SerializeField] protected EffectBase effectBase;

        protected bool playEffect = true;

        protected float TakeHit(HitReceiver hitReceiver, HitType type, float damage, Vector3? position = null, Vector3? normal = null) {
            if (IsInitialized()) {
                Quaternion rotation = normal.HasValue ? Quaternion.LookRotation(normal.Value, Vector3.up) : transform.rotation;
                if (playEffect && effectBase) {
                    effectBase.CreateAndPlay(position ?? transform.position, rotation);
                }
                return hitReceiver.TakeHit(this, type, damage);
            }
            return 0;
        }

        protected float TakeHit(HitReceiver hitReceiver, HitType type, float damage, Vector3? position = null, Vector3? normal = null, float delay = 0f) {
            if (IsInitialized()) {
                Quaternion rotation = normal.HasValue ? Quaternion.LookRotation(normal.Value, Vector3.up) : transform.rotation;
                if (playEffect && effectBase) {
                    effectBase.CreateAndPlay(position ?? transform.position, rotation);
                }
                if (delay > 0f) {
                    return hitReceiver.TakeHit(this, type, damage, delay);
                } else {
                    return hitReceiver.TakeHit(this, type, damage);
                }
            }
            return 0;
        }


    }

}
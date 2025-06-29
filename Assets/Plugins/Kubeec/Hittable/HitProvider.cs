using System;
using System.Collections;
using UnityEngine;

namespace Kubeec.Hittable {

    public class HitProvider : EnableDisableInitableDisposable {

        public event Action<HitInfo> onSendHit;

        public float damageMultiplier { get; set; } = 1f;

        public GameObject Owner {
            get {
                if (owner == null) {
                    owner = gameObject;
                }
                return owner;
            }
            set {
                owner = value;
            }
        }

        GameObject owner;

        public static HitInfo CreateHit(HitReceiver hitReceiver, HitProvider hitProvider, HitType type, float damage, Vector3? position = null, Vector3? normal = null) {
            HitInfo info = new HitInfo();
            info.hitReceiver = hitReceiver;
            info.hitProvider = hitProvider;
            info.type = type;
            info.damage = type == HitType.Heal ? -damage : damage;
            info.position = position;
            info.normal = normal;
            return info;
        }

        protected float SendHit(HitInfo info) {
            if (IsInitialized() && info.hitReceiver != null) {
                info.damage *= damageMultiplier;
                float damage = info.hitReceiver.TakeHit(info);
                onSendHit?.Invoke(info);
                return damage;
            }
            return 0;
        }

        protected HitInfo CreateHit(HitReceiver hitReceiver, HitType type, float damage, Vector3? position = null, Vector3? normal = null) {
            HitInfo info = CreateHit(hitReceiver, this, type, damage, position, normal);
            return info;
        }

        protected HitInfo CreateHit(HitReceiver hitReceiver, HitType type, float damage, Vector3? position = null, Vector3? normal = null, float delay = 0f) {
            HitInfo hitInfo = CreateHit(hitReceiver, type, damage, position, normal);
            hitInfo.delay = delay > 0f? delay : null;
            return hitInfo;
        }


    }

}
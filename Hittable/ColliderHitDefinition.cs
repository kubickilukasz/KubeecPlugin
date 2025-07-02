using UnityEngine;

namespace Kubeec.Hittable {

    [System.Serializable]
    public class ColliderHitDefinition {
        public Collider collider;
        public EffectBase effectBaseNotEnough;
        public EffectBase effectBaseReachMinForce;
        public HitType hitType;
        public float minForce = 1f;
        public float damagePerForce = 1f;
        public float maxDamage = -1f;
        public bool useStaticDamage = false;
        public float staticDamage = 1f;
    }

}

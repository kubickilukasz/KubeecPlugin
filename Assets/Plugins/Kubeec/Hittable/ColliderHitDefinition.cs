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
    }

}

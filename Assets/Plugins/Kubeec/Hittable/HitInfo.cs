using UnityEngine;

namespace Kubeec.Hittable {

    public struct HitInfo {
        public HitReceiver hitReceiver;
        public HitProvider hitProvider;
        public HitType type;
        public float damage;
        public Vector3? position;
        public Vector3? normal;
        public float? delay;
    }

}

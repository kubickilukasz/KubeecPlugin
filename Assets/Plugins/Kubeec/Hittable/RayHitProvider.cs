using UnityEngine;

namespace Kubeec.Hittable {

    public class RayHitProvider : HitProvider {

        [SerializeField] HitType type;
        [SerializeField] float baseDamage;
        [SerializeField] AnimationCurve damagerPerDistance;

        [Space]
        [SerializeField] LayerMask layerMask;
        [SerializeField] float distanceRaycast = 0.1f;
        [SerializeField] Transform startRaycast;
        [SerializeField] int interval = 3;
        [SerializeField] bool manualRaycast = false;

        RaycastHit info;

        void OnDrawGizmosSelected() {
            if (startRaycast) {
                Gizmos.DrawRay(startRaycast.position, startRaycast.forward * distanceRaycast);
            }
        }

        private void FixedUpdate() {
            if (!IsInitialized() || manualRaycast || (interval > 0 && Time.frameCount % interval != 0)) {
                return;
            }
            if (Raycast(out HitInfo hitInfo)) {
                SendHit(hitInfo);
            }
        }

        public bool CanRaycast() {
            return IsInitialized() && manualRaycast && (interval == 0 || Time.frameCount % interval == 0);
        }

        public bool TryRaycast(out HitInfo hitInfo) {
            if (!CanRaycast()) {
                hitInfo = new();
                return false;
            }
            return Raycast(out hitInfo);
        }

        bool Raycast(out HitInfo hitInfo) {
            if (Physics.Raycast(startRaycast.position, startRaycast.forward, out info, distanceRaycast, layerMask)) {
                if (info.collider.TryGetComponent(out HitReceiver hitReceiver)){
                    float damage = baseDamage * damagerPerDistance.Evaluate(Vector3.Distance(info.point, startRaycast.position) / distanceRaycast);
                    hitInfo = CreateHit(hitReceiver, type, damage, info.point, info.normal);
                    return true; 
                }
            }
            hitInfo = new HitInfo();
            return false;
        }

    }

}

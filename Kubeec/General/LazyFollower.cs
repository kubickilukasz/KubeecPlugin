using UnityEngine;

namespace Kubeec.General {

    public class LazyFollower : EnableDisableInitableDisposable {

        public Transform target;

        [SerializeField] float smooth;
        [SerializeField] Vector3 localPositionOffset;

        Vector3 velocity;

        void LateUpdate() {
            if (IsInitialized() && target) {
                transform.position = Vector3.SmoothDamp(transform.position, target.TransformPoint(localPositionOffset), ref velocity, smooth);
            }
        }

    }

}

using UnityEngine;

namespace Kubeec.VR.Player {

    public class EnableBasedView : EnableDisableInitableDisposable {
        
        [SerializeField] Behaviour[] enableComponent;
        [SerializeField] Transform pivot;
        [SerializeField] float maxAngle = 60f;
        [SerializeField] float maxDistance = 1f;

        LocalPlayerReference playerReference;
        float sqrMaxDistance;

        void Update() {
            if (IsInitialized()) {
                SetEnable(GetAngle() <= maxAngle && GetSqrDistance() <= maxDistance);
            }
        }

        protected override void OnInit(object data) {
            playerReference = LocalPlayerReference.instance;
            sqrMaxDistance = maxDistance * maxDistance;
        }

        protected override void OnDispose() {
            SetEnable(false);
        }

        float GetAngle() {
            Vector3 direction = pivot ? pivot.forward : transform.forward;
            return Vector3.Angle(playerReference.Camera.transform.forward, direction);
        }

        float GetSqrDistance() {
            Vector3 position = pivot ? pivot.position : transform.position;
            return (playerReference.Camera.transform.position - position).sqrMagnitude;
        }

        void SetEnable(bool enable) {
            for (int i = 0; i < enableComponent.Length; i++) {
                enableComponent[i].enabled = enable;
            }
        }

    }

}

using UnityEngine;

namespace Kubeec.VR.Player {

    public class LookAtCamera : EnableDisableInitableDisposable {

        [SerializeField] Camera overrideCamera;
        Camera currentCamera;

        void LateUpdate() {
            if (!IsInitialized()) {
                return;
            }
            transform.rotation = Quaternion.LookRotation(currentCamera.transform.position - transform.position, Vector3.up);
        }

        protected override void OnInit(object data) {
            if (data != null && data is Camera camera) {
                currentCamera = camera;
            } else if (overrideCamera != null) {
                currentCamera = overrideCamera;
            } else {
                currentCamera = LocalPlayerReference.instance.Camera;
            }
            if (currentCamera == null) {
                Dispose();
            } else {
                LateUpdate();
            }
        }

    }
}

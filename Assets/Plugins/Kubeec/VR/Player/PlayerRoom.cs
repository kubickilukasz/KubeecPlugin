using NaughtyAttributes;
using UnityEngine;

namespace Kubeec.VR.Player {

    public class PlayerRoom : Initable<object> {

        [SerializeField] LayerMask playerRoomLayerMask;

        Camera camera;
        LayerMask defaultLayerMask;

        [Button]
        public void Show(float? rotation = null) {
            Debug.Log(rotation);
            Init();
            if (rotation.HasValue) {
                transform.rotation = Quaternion.Euler(0f, rotation.Value, 0f);
            }
            camera.cullingMask = playerRoomLayerMask;
        }

        [Button]
        public void Hide() {
            camera.cullingMask = defaultLayerMask;
        }

        protected override void OnInit(object data) {
            camera = LocalPlayerReference.instance.Camera;
            defaultLayerMask = camera.cullingMask;
        }

    }

}

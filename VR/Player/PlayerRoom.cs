using NaughtyAttributes;
using UnityEngine;

namespace Kubeec.VR.Player {

    public class PlayerRoom : Initable<object> {

        [Button]
        public void Show(float? rotation = null) {
            Init();
            if (rotation.HasValue) {
                transform.rotation = Quaternion.Euler(0f, rotation.Value, 0f);
            }
        }

        [Button]
        public void Hide() {
            Init();
        }

    }

}

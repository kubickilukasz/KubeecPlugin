using Kubeec.VR.Outline;
using UnityEngine;

namespace Kubeec.VR.Player {
    public class LocalPlayerReference : Singleton<LocalPlayerReference> {

        [SerializeField] Camera camera;
        public Camera Camera => camera;

        [SerializeField] OutlineController outlineController;
        public OutlineController OutlineController => outlineController;

    }
}

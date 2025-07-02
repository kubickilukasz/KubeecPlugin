using Kubeec.VR.Player;
using UnityEngine;

namespace Kubeec.VR {

    public abstract class SceneDeathPlayerHandler : EnableDisableInitableDisposable {

        public static SceneDeathPlayerHandler Instance { get; private set; }

        public abstract PlayerSpace GetPlayerSpace();
        public abstract void HandleDeath(DeathPlayerHandler deathPlayer);

        protected override void OnInit(object data) {
            Instance = this;
        }

        protected override void OnDispose() {
            if (Instance == this) {
                Instance = null;
            }
        }

    }

}

using UnityEngine;

namespace Kubeec.NPC {

    public class PlayerReference : EnableDisableInitableDisposable {

        [SerializeField] Transform[] centers;

        public Transform[] Points => centers;

        protected override void OnInit(object data) {
            NPCContainer.instance.RegisterPlayer(this);
        }

        protected override void OnDispose() {
            NPCContainer.instance.UnregisterPlayer(this);
        }

    }

}

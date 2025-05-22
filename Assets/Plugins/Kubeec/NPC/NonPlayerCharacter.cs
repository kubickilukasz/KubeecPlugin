using UnityEngine;

namespace Kubeec.NPC {

    public class NonPlayerCharacter : EnableDisableInitableDisposable {

        [SerializeField] protected NPCMove move;
        [SerializeField] protected NPCBehaviour behaviour;

        public NPCMove Move => move;
        public NPCBehaviour Behaviour => behaviour;

        protected override void OnInit(object data) {
            move.Init(this);
            behaviour.Init(this);
            NPCContainer.instance.RegisterNPC(this);
        }

        protected override void OnDispose() {
            NPCContainer.instance.UnregisterNPC(this);
            behaviour.Dispose();
            move.Dispose();
        }

    }

}

using UnityEngine;

namespace Kubeec.NPC {

    public class NonPlayerCharacter : EnableDisableInitableDisposable {

        [SerializeField] protected NPCMove move;
        [SerializeField] protected NPCBehaviour behaviour;

        public NPCMove Move => move;
        public NPCBehaviour Behaviour => behaviour;

        NPCInitable[] nPCInitables;

        protected override void OnInit(object data) {
            nPCInitables = GetComponentsInChildren<NPCInitable>(true);
            foreach (NPCInitable nPCInitable in nPCInitables) {
                nPCInitable.Init(this);
            }
            NPCContainer.instance.RegisterNPC(this);
        }

        protected override void OnDispose() {
            NPCContainer.instance.UnregisterNPC(this);
            foreach (NPCInitable nPCInitable in nPCInitables) {
                nPCInitable.Dispose();
            }
        }

    }

}

using UnityEngine;

namespace Kubeec.NPC {

    public class PlayerReference : EnableDisableInitableDisposable {

        [SerializeField] Transform[] centers;

        public Transform[] Points => centers;

        public Vector3 Position(Vector3 vector3) {
            float current = float.MaxValue;
            Vector3 pos = transform.position;
            foreach (Transform t in centers) {
                float c = (t.position - vector3).sqrMagnitude;
                if (c < current) {
                    current = c;
                    pos = t.position;
                }
            }
            return pos;
        }

        public float Distance(Vector3 vector3) {
            float current = float.MaxValue;
            foreach (Transform t in centers) {
                float c = Vector3.Distance(t.position, vector3);
                if (c < current) {
                    current = c;
                }
            }
            return current;
        }

        protected override void OnInit(object data) {
            NPCContainer.instance.RegisterPlayer(this);
        }

        protected override void OnDispose() {
            if (NPCContainer.instanceExist) {
                NPCContainer.instance.UnregisterPlayer(this);
            }
        }

    }

}

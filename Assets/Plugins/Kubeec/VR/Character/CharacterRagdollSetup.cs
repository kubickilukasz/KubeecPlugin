using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kubeec.VR.Character {

    [System.Serializable]
    public class CharacterRagdollSetup {

        [SerializeField] List<Rigidbody> rigidbodies = new();
        [SerializeField] List<CharacterJoint> joints = new();
        [SerializeField] List<Collider> colliders = new();

        public void Setup(Transform parentRig) {
            rigidbodies = parentRig.GetComponentsInChildren<Rigidbody>(true).ToList();
            joints = parentRig.GetComponentsInChildren<CharacterJoint>(true).ToList();
            colliders = parentRig.GetComponentsInChildren<Collider>(true).ToList();
        }

        public void Set(bool ragdoll, bool withColliders) {
            withColliders = true;
            if (withColliders) {
                colliders.ForEach(c => c.enabled = true);
                rigidbodies.ForEach(r => r.isKinematic = !ragdoll);
            } else {
                colliders.ForEach(c => c.enabled = false);
                rigidbodies.ForEach(r => r.isKinematic = true);
            }
        }

    }

}

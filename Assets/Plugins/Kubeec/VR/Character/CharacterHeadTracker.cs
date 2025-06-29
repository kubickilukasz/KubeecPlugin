using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kubeec.VR.Character {
    public class CharacterHeadTracker : Initable<Animator> {

        [SerializeField] bool IsDebug;

        Animator animator;
        Vector3 _startedPosition;

        public Vector3 startedPosition => _startedPosition;

        public void SetPosition(Vector3 position) {
#if UNITY_EDITOR
            if (IsDebug) {
                return;
            }
#endif
            transform.position = position;
        }

        public void SetRotation(Quaternion rotation) {
#if UNITY_EDITOR
            if (IsDebug) {
                return;
            }
#endif
            transform.rotation = rotation;
        }

        public void UpdateHead() {
            if (animator != null) {
                animator.SetLookAtWeight(1f);
                animator.SetLookAtPosition(transform.position + transform.forward);
            }
        }

        protected override void OnInit(Animator animator) {
            _startedPosition = transform.position;
            this.animator = animator;
        }

    }
}

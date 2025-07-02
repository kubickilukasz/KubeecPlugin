using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kubeec.VR.Character {

    [RequireComponent(typeof(Animator))]
    public class CharacterAnimatorReference : MonoBehaviour {

        List<ICharacterAnimatorEventSender> characterAnimatorEventSenders = new List<ICharacterAnimatorEventSender>();

        Animator _animator;

        public Animator animator {
            get {
                if (_animator == null) {
                    _animator = GetComponent<Animator>();
                }
                return _animator;
            }
        }

        void OnAnimatorMove() {
            for (int i = 0; i < characterAnimatorEventSenders.Count; i++) {
                characterAnimatorEventSenders[i].OnAnimatorMoveOther();
            }
        }

        void OnAnimatorIK(int layerIndex) {
            for (int i = 0; i < characterAnimatorEventSenders.Count; i++) {
                characterAnimatorEventSenders[i].OnAnimatorIKOther(layerIndex);
            }
        }

        public void Register(ICharacterAnimatorEventSender characterAnimatorEventSender) {
            if (!characterAnimatorEventSenders.Contains(characterAnimatorEventSender)) {
                characterAnimatorEventSenders.Add(characterAnimatorEventSender);
            }
        }

        public void Unregister(ICharacterAnimatorEventSender characterAnimatorEventSender) {
            if (characterAnimatorEventSenders.Contains(characterAnimatorEventSender)) {
                characterAnimatorEventSenders.Remove(characterAnimatorEventSender);
            }
        }

    }

    public interface ICharacterAnimatorEventSender {
        public void OnAnimatorMoveOther();
        public void OnAnimatorIKOther(int layerIndex);
    }

}
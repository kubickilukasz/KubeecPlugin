using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kubeec.VR.Character {

    [System.Serializable]
    public class CharacterHandState {

        [Range(-0.3f, 2f)] public float thumbValue = 0.75f;
        [Range(-0.3f, 2f)] public float pointerValue = 0.75f;
        [Range(-0.3f, 2f)] public float fingersValue = 0.75f;


        public CharacterHandState Lerp(CharacterHandState other, float t) {
            CharacterHandState newState = new CharacterHandState() {
                thumbValue = Mathf.Lerp(thumbValue, other.thumbValue, t),
                pointerValue = Mathf.Lerp(pointerValue, other.pointerValue, t),
                fingersValue = Mathf.Lerp(fingersValue, other.fingersValue, t)
            };
            return newState;
        }

    }

}

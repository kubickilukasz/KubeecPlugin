using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kubeec.VR.Character {

    [CreateAssetMenu(fileName = "HandInteraction", menuName = "ScriptableObjects/HandInteraction")]
    public class CharacterHandInteraction : ScriptableObject {

        [SerializeField] CharacterHandState inactiveState;
        [SerializeField] CharacterHandState activeState;
        [SerializeField] AnimationCurve transition;

        [Space]

        [SerializeField, Range(0, 1f)] float selectAffectThumb;
        [SerializeField, Range(0, 1f)] float selectAffectPointer;
        [SerializeField, Range(0, 1f)] float selectAffectFingers;
        [SerializeField, Range(0, 1f)] float gripAffectThumb;
        [SerializeField, Range(0, 1f)] float gripAffectPointer;
        [SerializeField, Range(0, 1f)] float gripAffectFingers;

        public float GetThumbState(float gripValue, float selectValue) {
            return GetState(inactiveState.thumbValue, activeState.thumbValue, GetT(gripValue, selectValue, gripAffectThumb, selectAffectThumb));
        }

        public float GetPointerState(float gripValue, float selectValue) {
            return GetState(inactiveState.pointerValue, activeState.pointerValue, GetT(gripValue, selectValue, gripAffectPointer, selectAffectPointer));
        }

        public float GetFingersState(float gripValue, float selectValue) {
            return GetState(inactiveState.fingersValue, activeState.fingersValue, GetT(gripValue, selectValue, gripAffectFingers, selectAffectFingers));
        }

        float GetState(float min, float max, float t) {
            return Mathf.Lerp(min, max, transition.Evaluate(t));
        }

        float GetT(float gripValue, float selectValue, float gripAffect, float selectAffect) {
            float value = gripValue * gripAffect + selectValue * selectAffect;
            return Mathf.Clamp01(value);
        }

    }

}

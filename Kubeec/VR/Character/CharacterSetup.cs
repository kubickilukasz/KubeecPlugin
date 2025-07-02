using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kubeec.VR.Character {
    public class CharacterSetup : MonoBehaviour {

        [SerializeField] Transform leftShoulder;
        [SerializeField] Transform rightShoulder;
        [SerializeField] Transform leftForearm;
        [SerializeField] Transform rightForearm;
        [SerializeField] Transform leftHand;
        [SerializeField] Transform rightHand;
        [SerializeField] Transform headInSkeleton;
        [SerializeField] Transform groundInSkeleton;
        [SerializeField] Transform rig;
        [SerializeField] Transform hips;
        [SerializeField] Renderer headRenderer;

        [Space]

        [SerializeField] CharacterRagdollSetup characterRagdollSetup;

        [Space]

        [SerializeField] CharacterHandSetup leftHandSetup;
        [SerializeField] CharacterHandSetup rightHandSetup;

        [Space]

        [SerializeField] Vector3 headOffset;

        public CharacterHandSetup LeftHandSetup => leftHandSetup;
        public CharacterHandSetup RightHandSetup => rightHandSetup;
        public CharacterRagdollSetup RagdollSetup => characterRagdollSetup;
        public Vector3 headLocalPosition => headInSkeleton.position - transform.position + headOffset;
        public Vector3 headPosition => headInSkeleton.position;
        public Quaternion headRotation => headInSkeleton.rotation;
        public Vector3 groundLocalPosition => groundInSkeleton.position - transform.position;
        public Vector3 handLeftOffset => leftHandSetup.OffsetPosition;
        public Vector3 handRightOffset => rightHandSetup.OffsetPosition;

        public Transform LeftHand => leftHand;
        public Transform RightHand => rightHand;
        public Transform LeftShoulder => leftShoulder;
        public Transform RightShoulder => rightShoulder;
        public Transform LeftForearm => leftForearm;
        public Transform RightForearm => rightForearm;
        public Transform Head => headInSkeleton;
        public Transform Hips => hips;

        public Renderer HeadRenderer => headRenderer;

        public void Prepare() {
            leftHandSetup.shoulderReference = leftShoulder;
            rightHandSetup.shoulderReference = rightShoulder;
        }

#if UNITY_EDITOR
        [Button]
        public void ResetLeftHand() {
            leftHandSetup.Reset(true);
            SaveAsset();
        }

        [Button]
        public void ResetRightHand() {
            rightHandSetup.Reset(false);
            SaveAsset();
        }

        [Button]
        public void ResetBothHands() {
            leftHandSetup.Reset(true);
            rightHandSetup.Reset(false);
            SaveAsset();
        }

        [Button]
        public void SetupRagdoll() {
            characterRagdollSetup.Setup(rig);
            SaveAsset();
        }

        void SaveAsset() {
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
            UnityEditor.AssetDatabase.SaveAssets();
        }
#endif

        public void UpdateLeftShoulder(Vector3 realHandPosition) {
            UpdateShoulder(leftHand.position, realHandPosition, leftShoulder);
        }

        public void UpdateRightShoulder(Vector3 realHandPosition) {
            UpdateShoulder(rightHand.position, realHandPosition, rightShoulder);
        }

        void UpdateShoulder(Vector3 handPosition, Vector3 realHandPosition, Transform shoulder) {
            Vector3 realDir = realHandPosition - shoulder.position;
            Vector3 handDir = handPosition - shoulder.position;
            if (handDir.magnitude < realDir.magnitude) {
                shoulder.position += (realHandPosition - handPosition);
            }
        }

    }
}

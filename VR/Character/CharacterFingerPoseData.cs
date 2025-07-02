using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Kubeec.VR.Character {
    [CreateAssetMenu(fileName = "FingerPose", menuName = "ScriptableObjects/FingerPose")]
    public class CharacterFingerPoseData : ScriptableObject {

        const string folder = "FingerPoses";

        public CharacterFingerPose fingerPose;

#if UNITY_EDITOR

        [Button]
        [ContextMenu("Copy from selection")]
        public void CopyFromSelection() {
            GameObject root = Selection.activeGameObject;
            if (root == null) {
                Debug.LogError("Selection is empty!");
                return;
            }

            List<Transform> transforms = root.GetComponentsInChildrenInOrder<Transform>();
            fingerPose.rotations = new Vector3[transforms.Count];

            for (int i = 0; i < transforms.Count; i++) {
                fingerPose.rotations[i] = transforms[i].localRotation.eulerAngles;
            }

            EditorUtility.SetDirty(this);
        }

        [Button]
        [ContextMenu("SetRotation to selection")]
        public void SetToSelection() {
            GameObject root = Selection.activeGameObject;
            if (root == null) {
                Debug.LogError("Selection is empty!");
                return;
            }

            List<Transform> transforms = root.GetComponentsInChildrenInOrder<Transform>();
            if (transforms.Count != fingerPose.rotations.Length) {
                Debug.LogError("Diffrent sizes of arrays");
                return;
            }

            for (int i = 0; i < transforms.Count; i++) {
                transforms[i].localRotation = Quaternion.Euler(fingerPose.rotations[i]);
            }

            EditorUtility.SetDirty(root);
        }
#endif

        public static CharacterFingerPose GetDataFromFile(string name) {
            string fullPath = Path.Combine(folder, name).GetPathToResourcesData();
            CharacterFingerPoseData data = Resources.Load<CharacterFingerPoseData>(fullPath);
            return data.fingerPose;
        }

    }

}

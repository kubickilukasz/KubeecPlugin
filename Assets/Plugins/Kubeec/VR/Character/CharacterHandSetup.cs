using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kubeec.VR.Character {

    [Serializable]
    public class CharacterHandSetup {

        [NonSerialized] public Transform shoulderReference;

        [SerializeField] Vector3 offsetPosition;
        [SerializeField] Vector3 offsetRotation;
        [SerializeField] float multiplier = 1f;

        [SerializeField] Transform rootThumb;
        [SerializeField] Transform rootRing;
        [SerializeField] Transform rootPinky;
        [SerializeField] Transform rootMiddle;
        [SerializeField] Transform rootPoint;

        [SerializeField] Transform topPointFinger;

        [SerializeField] List<Transform> transformsThumb = new List<Transform>();
        [SerializeField] List<Transform> transformsRing = new List<Transform>();
        [SerializeField] List<Transform> transformsPinky = new List<Transform>();
        [SerializeField] List<Transform> transformsMiddle = new List<Transform>();
        [SerializeField] List<Transform> transformsPoint = new List<Transform>();

#if UNITY_EDITOR
        public CharacterHandState inOutFingers = new CharacterHandState();
#endif

        public float Mutliplier => multiplier;
        public Vector3 OffsetPosition => offsetPosition;
        public Quaternion OffsetRotation => Quaternion.Euler(offsetRotation);

        public void Reset(bool handLeft) {

            Transform[] transforms = new Transform[] {
            rootThumb, rootRing, rootPinky, rootMiddle, rootPoint
        };
            List<Transform>[] transformsList = new List<Transform>[] {
            transformsThumb, transformsRing, transformsPinky, transformsMiddle, transformsPoint
        };


            for (int i = 0; i < transforms.Length; i++) {
                if (transforms[i] != null) {
                    transformsList[i].Clear();
                    transformsList[i].AddRange(transforms[i].GetComponentsInChildrenInOrder<Transform>());
                }
            }
        }

        public Transform GetTopPointFinger() {
            if (topPointFinger == null) {
                if (transformsPoint.Count == 0) {
                    return null;
                }
                topPointFinger = transformsPoint[Mathf.Max(transformsPoint.Count - 2, 0)];
            }
            return topPointFinger;
        }

        public Transform GetWristPoint() {
            return rootThumb.parent;
        }

    }

}

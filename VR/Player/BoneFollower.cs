using Kubeec.VR.Character;
using UnityEngine;

namespace Kubeec.VR.Player {

    public class BoneFollower : EnableDisableInitableDisposable {

        [SerializeField] PlayerController controller;
        [SerializeField] float localOffset = 0.2f;
        [SerializeField] Vector3 localOffsetPosition;
        [SerializeField] Quaternion localOffsetRotation;
        [SerializeField] BoneType boneType;

        CharacterSetup setup;
        Transform currentTransformStart;
        Transform currentTransformEnd;

        void LateUpdate() {
            if (IsInitialized()) {
                Vector3 diff = currentTransformEnd.position - currentTransformStart.position;
                transform.SetPositionAndRotation(
                    currentTransformStart.position + (diff.normalized * localOffset), 
                    Quaternion.LookRotation(diff) * localOffsetRotation
                    );
                transform.position += transform.rotation * localOffsetPosition;
            }
        }

        protected override void OnInit(object data) {
            controller.onChangeCharacter += ChangeCharacterSetup;
        }

        protected override void OnDispose() {
            if (controller) {
                controller.onChangeCharacter -= ChangeCharacterSetup;
            }
        }

        void ChangeCharacterSetup(CharacterSetup setup) {
            this.setup = setup;
            currentTransformStart = GetTransformStart(boneType);
            currentTransformEnd = GetTransformEnd(boneType);
        }

        Transform GetTransformStart(BoneType boneType) {
            switch (boneType) {
                case BoneType.LeftShoulder: return setup.LeftShoulder;
                case BoneType.LeftForearm: return setup.LeftForearm;
                case BoneType.LeftHand: return setup.LeftHand;
                case BoneType.RightShoulder: return setup.RightShoulder;
                case BoneType.RightForearm: return setup.RightForearm;
                case BoneType.RightHand: return setup.RightHand;
                case BoneType.Hips: return setup.Hips;
                default: return setup.Head;
            }
        }

        Transform GetTransformEnd(BoneType boneType) {
            switch (boneType) {
                case BoneType.LeftShoulder: return setup.LeftForearm;
                case BoneType.LeftForearm: return setup.LeftHand;
                case BoneType.LeftHand: return setup.LeftHand.GetChild(0);
                case BoneType.RightShoulder: return setup.RightForearm;
                case BoneType.RightForearm: return setup.RightHand;
                case BoneType.RightHand: return setup.RightHand.GetChild(0);
                case BoneType.Hips: return setup.Hips.GetChild(0);
                default: return setup.Head;
            }
        }

        public enum BoneType {
            LeftShoulder, LeftForearm, LeftHand, 
            RightShoulder, RightForearm, RightHand,
            Head, Hips
        }

    }

}

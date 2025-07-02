using Kubeec.VR.Interactions;
using System.Collections;
using UnityEngine;

namespace Kubeec.VR.Interactions {

    public class FreeHandHandler : HandHandler{

        public override HandHandlerData Get(Transform item, HandInteractor hand) {
            HandHandlerData output = new HandHandlerData {
                handPose = handPose,
                itemPosition = GetItemPosition(item, hand.controller.GetInputHandPosition()),
                handPosition = GetHandPosition(hand.controller.IsRightHand),
                itemRotation = Quaternion.identity,
                holdDirection = holdDirection,
                handRotation = Quaternion.identity,
                weight = weight
            };
            hand.controller.GetHandPositionAndRotationPalmByShoulder(holdDirection, ref output.handPosition, ref output.handRotation);
            return output;
        }

#if UNITY_EDITOR

        protected override Material GetDebugMaterial() {
            return Resources.Load("GhostHandMaterial2") as Material;
        } 

        static Material _material2;
        protected override Material material {
            get {
                if (_material2 == null) {
                    _material2 = GetDebugMaterial();
                }
                return _material2;
            }
        }

#endif

    }

}
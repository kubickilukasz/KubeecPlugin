using UnityEngine;


namespace Kubeec.VR.Interactions {

    public class FingerHandHandler : HandHandler {

        public override HandHandlerData Get(Transform item, HandInteractor hand) {
            Quaternion targetItemRotation = GetItemRotation(hand);
            HandHandlerData output = new HandHandlerData {
                handPose = handPose,
                itemPosition = GetItemPosition(item, hand.controller.GetInputHandPosition()),
                itemRotation = targetItemRotation,
                handPosition = GetHandPosition(hand.controller.IsRightHand),
                handRotation = hand.controller.GetHandTargetRotation(),
                holdDirection = Vector3.zero,
                weight = weight
            };
            hand.controller.GetHandPositionAndRotationTopFinger(transform.position, ref output.handPosition, ref output.handRotation);
            return output;
        }

    }

}

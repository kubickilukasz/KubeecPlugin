using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kubeec.VR.Interactions {

    [ExecuteAlways]
    public class StaticHandHandler : HandHandler{

        public override HandHandlerData Get(Transform item, HandInteractor hand) {
            Quaternion targetItemRotation = GetItemRotation(hand);
            HandHandlerData output = new HandHandlerData {
                handPose = handPose,
                itemPosition = GetItemPosition(item, hand.controller.GetInputHandPosition()),
                itemRotation = targetItemRotation,
                handPosition = GetHandPosition(hand.controller.IsRightHand),
                handRotation = (transform.parent.rotation * Quaternion.Inverse(targetItemRotation)) * GetHandRotation(hand),
                holdDirection = holdDirection,
                weight = weight
            };
            hand.controller.GetHandPositionAndRotationPalm(holdDirection, ref output.handPosition, ref output.handRotation);
            return output;
        }


    }

    
}

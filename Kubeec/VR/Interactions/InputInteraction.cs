using System;
using UnityEngine;

namespace Kubeec.VR.Interactions {

    public enum InputInteraction {
        Grip, Select, Both
    }

    public static class InputInteractionExtensions {

        public static void GetInputs(this InputInteraction inputInteraction, 
            out Func<HandInteractor, bool> isDown, out Func<HandInteractor, bool> isPressed, out Func<HandInteractor, float> getValue) {
            switch (inputInteraction) {
                case InputInteraction.Grip:
                    isPressed = (HandInteractor x) => x.controller.IsGripPressed();
                    isDown = (HandInteractor x) => x.controller.IsGripDown();
                    getValue = (HandInteractor x) => x.controller.GetGripValue();
                    break;
                case InputInteraction.Select:
                    isPressed = (HandInteractor x) => x.controller.IsSelectPressed();
                    isDown = (HandInteractor x) => x.controller.IsSelectDown();
                    getValue = (HandInteractor x) => x.controller.GetSelectValue();
                    break;
                case InputInteraction.Both:
                    isPressed = (HandInteractor x) => x.controller.IsSelectPressed() || x.controller.IsGripPressed();
                    isDown = (HandInteractor x) => x.controller.IsSelectDown() || x.controller.IsGripDown();
                    getValue = (HandInteractor x) => Mathf.Max(x.controller.GetGripValue(), x.controller.GetSelectValue());
                    break;
                default:
                    isPressed = (HandInteractor x) => false;
                    isDown = (HandInteractor x) => false;
                    getValue = (HandInteractor x) => 0f;
                    break;
            }
        }

    }

}

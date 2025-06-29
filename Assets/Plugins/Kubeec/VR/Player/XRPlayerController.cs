using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class XRPlayerController : PlayerControllerBase {

    [SerializeField] Transform head;
    [SerializeField] Transform leftHand;
    [SerializeField] Transform rightHand;

    [Space]

    [SerializeField] InputActionReference joystickLeft;
    [SerializeField] InputActionReference joystickRight;
    [SerializeField] InputActionReference pressLeftGrip;
    [SerializeField] InputActionReference pressRightGrip;
    [SerializeField] InputActionReference pressLeftSelect;
    [SerializeField] InputActionReference pressRightSelect;

    [SerializeField] float inputSensitiveToRotate = 0.5f;
    [SerializeField] float angleToRotate = 30f;

    bool isChangingRotation = false;

    void Start() {
        Application.onBeforeRender += OnUpdateHead;
    }

    void OnDestroy() {
        Application.onBeforeRender -= OnUpdateHead;
    }

    public override Vector2 GetJoyLeftHand() {
        return joystickLeft.action.ReadValue<Vector2>();
    }

    public override Vector2 GetJoyRightHand() {
        return joystickRight.action.ReadValue<Vector2>();
    }

    public override Vector3 GetHeadPosition() {
        return head.position;
    }

    public override Vector3 GetPositionLeftHand() {
        return leftHand.position;
    }

    public override Vector3 GetPositionRightHand() {
        return rightHand.position;
    }

    public override float GetPressValueLeftGrip() {
        return pressLeftGrip.action.ReadValue<float>();
    }

    public override float GetPressValueLeftSelect() {
        return pressLeftSelect.action.ReadValue<float>();
    }

    public override float GetPressValueRightGrip() {
        return pressRightGrip.action.ReadValue<float>();
    }

    public override float GetPressValueRightSelect() {
        return pressRightSelect.action.ReadValue<float>();
    }

    public override Vector3 GetRootPosition() {
        return transform.position;
    }

    public override Quaternion GetHeadRotation() {
        return head.rotation;
    }

    public override Quaternion GetRotationLeftHand() {
        return leftHand.rotation;
    }

    public override Quaternion GetRotationRightHand() {
        return rightHand.rotation;
    }

    public override void SetPosition(Vector3 newPosition) {
        transform.position = newPosition;
    }

    protected override void OnUpdate() {
        Vector2 v3 = GetJoyRightHand();
        float value = v3.x;
        float absValue = Mathf.Abs(value);
        if (!isChangingRotation) {
            if (absValue >= inputSensitiveToRotate) {
                isChangingRotation = true;
                value = value > 0f ? 1f : -1f;
                transform.rotation *= Quaternion.Euler(0f, value * angleToRotate, 0f);
            }
        } else if (absValue < inputSensitiveToRotate) {
            isChangingRotation = false;
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Use this for debuging only
/// </summary>
public class WindowsPlayerController : PlayerControllerBase {

    const float maxAngleX = 65;

    [SerializeField] Vector3 headOffset;
    [SerializeField] Vector3 leftHandOffset;
    [SerializeField] Vector3 rightHandOffset;
    [SerializeField] float mouseSpeed = 10f;
    [SerializeField] Transform head;
    [SerializeField] Vector3 rot;
    [SerializeField] bool isLeftHand = false;
    [SerializeField] bool updateCameraOnBeforeRender = true;
    [SerializeField] bool updateControllersOnBeforeRender = false;

    Transform leftHand;
    Transform rightHand;
    Vector3 newPosition;
    
    void Start() {
        RefreshHand();
        rot = head.rotation.eulerAngles;
        Cursor.lockState = CursorLockMode.Locked;
        Application.onBeforeRender += UpdateHead;
    }

    void OnDestroy() {
        Application.onBeforeRender -= UpdateHead;
    }

    void OnDrawGizmos() {
        if (leftHand) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(leftHand.transform.position, Vector3.one * 0.05f);
            Gizmos.DrawCube(rightHand.transform.position, Vector3.one * 0.05f);
        }
    }

    public override Vector2 GetJoyLeftHand() {
        return Input.GetButton("DebugWindows") ? Vector2.zero : new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    public override Vector2 GetJoyRightHand() {
        return Vector2.zero;
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
        return isLeftHand && Input.GetButton("Fire2") ? 1 : 0;
    }

    public override float GetPressValueLeftSelect() {
        return isLeftHand && Input.GetButton("Fire1") ? 1 : 0;
    }

    public override float GetPressValueRightGrip() {
        return !isLeftHand && Input.GetButton("Fire2") ? 1 : 0;
    }

    public override float GetPressValueRightSelect() {
        return !isLeftHand && Input.GetButton("Fire1") ? 1 : 0;
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

    Transform SetTransform(ref Transform created, Vector3 position, Transform parent, string name) {
        if (created == null) {
            created = (new GameObject(name)).transform;
        } else {
            created.name = name;
        }
        created.SetParent(parent);
        created.localPosition = position;
        return created;
    }

    void RefreshHand() {
        SetTransform(ref head, headOffset, transform, "Head");
        Vector3 _rightHandOffset = !isLeftHand ? rightHandOffset : leftHandOffset;
        Vector3 _leftHandOffset = isLeftHand ? rightHandOffset : leftHandOffset;
        if (isLeftHand) {
            _rightHandOffset.x = -_rightHandOffset.x;
            _leftHandOffset.x = -_leftHandOffset.x;
        }
        SetTransform(ref leftHand, _leftHandOffset, head, "Left Hand");
        SetTransform(ref rightHand, _rightHandOffset, head, "Right Hand");
        if (!updateControllersOnBeforeRender) {

        }
    }

    protected override void OnFixedUpdate() {
        //UpdateHeadOutput(false);
        //transform.position = newPosition;
    }

    protected override void OnUpdate() {
        base.OnUpdate();
        if (Input.GetButton("DebugWindows")) {
            Quaternion rot = Quaternion.Euler(0, head.eulerAngles.y, 0);
            head.localPosition = head.localPosition + rot * (new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * Time.deltaTime) * 5f;
            head.localPosition = new Vector3(head.localPosition.x, headOffset.y, head.localPosition.z);
        }
        UpdateHeadInputs();

        if (Input.GetKeyDown(KeyCode.Space)) {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
        }

        if(Input.GetKeyDown(KeyCode.Q)){
            isLeftHand = !isLeftHand;
            RefreshHand();
        }

        UpdateHeadOutput(false);
    }

    public override void SetPosition(Vector3 newPosition) {
        this.newPosition = newPosition;
        transform.position = newPosition;
    }

    void UpdateHeadInputs() {
        rot.y = head.rotation.eulerAngles.y;
        rot.x -= Input.GetAxis("Mouse Y") * mouseSpeed * Time.deltaTime;
        rot.y += Input.GetAxis("Mouse X") * mouseSpeed * Time.deltaTime;
        rot.x = Mathf.Clamp(rot.x, -maxAngleX, maxAngleX);
        rot.z = head.rotation.eulerAngles.z;


    }

    void UpdateHeadOutput(bool onBeforeRender = false) {
        if (!onBeforeRender && !updateControllersOnBeforeRender) {
            leftHand.SetParent(head, true);
            rightHand.SetParent(head, true);
            head.rotation = Quaternion.Euler(rot);
            leftHand.SetParent(transform, true);
            rightHand.SetParent(transform, true);
        } else {
            head.rotation = Quaternion.Euler(rot);
        }
    }

    void UpdateHead() {
        if (updateCameraOnBeforeRender) {
            UpdateHeadInputs();
            UpdateHeadOutput(true);
            OnUpdateHead();
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-6)] // this have to be before PlayerController, and before default Update
public abstract class PlayerControllerBase : Singleton<PlayerControllerBase> {

    public const float pressedThreshold = 0.6f;
    public event Action onShouldUpdateHead; 

    void FixedUpdate() {
        OnFixedUpdate();
    }

    void Update() {
        OnUpdate();
    }

    public virtual Vector3 GetBodyPosition() => transform.position;

    public static bool IsPressed(float value) { return value > pressedThreshold; }

    [SerializeField] protected List<Transform> vrModels = new List<Transform>();

    public virtual void SetDefaultVRModels(bool defaultModels = false) {
        foreach (Transform model in vrModels){
            model.gameObject.SetActive(defaultModels);
        }
    }

    public abstract void SetPosition(Vector3 newPosition);
    public abstract Vector3 GetRootPosition();
    public abstract Vector3 GetHeadPosition();
    public abstract Quaternion GetHeadRotation();
    public abstract Vector3 GetPositionLeftHand();
    public abstract Vector3 GetPositionRightHand();
    public abstract Quaternion GetRotationLeftHand();
    public abstract Quaternion GetRotationRightHand();

    public abstract Vector2 GetJoyLeftHand();
    public abstract Vector2 GetJoyRightHand();
    public abstract float GetPressValueLeftSelect();
    public abstract float GetPressValueRightSelect();
    public abstract float GetPressValueLeftGrip();
    public abstract float GetPressValueRightGrip();

    public bool IsPressedValueLeftSelect() => IsPressed(GetPressValueLeftSelect()); 
    public bool IsPressedValueRightSelect() => IsPressed(GetPressValueRightSelect());
    public bool IsPressedValueLeftGrip() => IsPressed(GetPressValueLeftGrip());
    public bool IsPressedValueRightGrip() => IsPressed(GetPressValueRightGrip());

    protected virtual void OnFixedUpdate() { }
    protected virtual void OnUpdate() { }
    protected void OnUpdateHead() {
        onShouldUpdateHead?.Invoke();
    }

}

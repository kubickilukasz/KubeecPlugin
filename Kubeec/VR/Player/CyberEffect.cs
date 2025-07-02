using Kubeec.Hittable;
using Kubeec.VR.Player;
using System;
using Unity.Netcode;
using UnityEngine;

public class CyberEffect : EnableDisableInitableDisposable, IAction{

    public event Action onAction;

    [SerializeField] NetworkObject networkObject;
    [SerializeField] Renderer renderer;
    [SerializeField] HitCollector collector;

    [Space]

    [SerializeField] float timeToHide = 0.8f;
    [SerializeField] float forcePerDamage = 0.005f;

    Vector4 leftRight = new Vector4(0, 0, 1, 1); //x
    Vector4 topBottom = new Vector4(0, 1, 0, 0); //z
    Transform camera;
    int valueShaderId;
    Vector4 currentValue;
    float timer;

    void Update() {
        if (IsInitialized()) {
            UpdateTransform();
            UpdateValue();
        }
    }

    protected override void OnInit(object data) {
        collector.onHit += OnHit;
        if (!networkObject.IsOwner) {
            Dispose();
            return;
        }
        valueShaderId = Shader.PropertyToID("_Borders");
        camera = LocalPlayerReference.instance?.Camera?.transform;
        renderer.gameObject.SetActive(true);
        timer = timeToHide;
    }

    protected override void OnDispose() {
        renderer.gameObject.SetActive(false);
        if (collector) {
            collector.onHit -= OnHit;
        }
    }

    public void UpdateTransform() {
        if (camera != null) {
            transform.position = camera.position;
            transform.rotation = camera.rotation;
        }
    }

    public void SetEffect(Vector2 direction, float damage) {
        Vector4 value;
        float mul = damage * forcePerDamage;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y)) {
            value = leftRight;
            value.x = direction.x > 0 ? -1 : 1;
            SetMaterialValue(value * mul);
        } else {
            value = topBottom;
            value.z = direction.y > 0 ? 1 : -1;
            SetMaterialValue(value * mul);
        }
    }

    void SetMaterialValue(Vector4 value) {
        currentValue = value;
        renderer.sharedMaterial.SetVector(valueShaderId, value);
    }

    void UpdateValue() {
        if (IsInitialized() && timer <= timeToHide) {
            currentValue = Vector4.Lerp(currentValue, Vector4.zero, timer / timeToHide);
            SetMaterialValue(currentValue);
            timer += Time.deltaTime;
        }
    }

    void OnHit(HitInfo info) {
        if (info.damage <= 0f) {
            return;
        }
        timer = 0f;
        if (info.position.HasValue) {
            Vector3 localPos = camera.InverseTransformPoint(info.position.Value);
            SetEffect(localPos, info.damage);
        } else {
            SetEffect(Vector2.down, info.damage);
        }
        onAction?.Invoke();
    }

}

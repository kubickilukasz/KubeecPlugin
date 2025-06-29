using System;
using UnityEngine;

public class AudioEffectState : AudioEffectBase, IStateFloat {

    public event Action<float> onChangeState;

    [SerializeField] Vector2 minMaxPitch;
    [SerializeField] Vector2 minMaxVolume;

    float state = 0f;
    float basePitch, baseVolume;

    public float GetState() {
        return state;
    }

    public void SetState(float value) {
        if (!IsInitialized()) {
            return;
        }
        state = value;
        onChangeState?.Invoke(state);
        if (minMaxPitch.sqrMagnitude > 0f) {
            source.audioSource.pitch = basePitch + Mathf.Lerp(minMaxPitch.x, minMaxPitch.y, value);
        }
        if (minMaxVolume.sqrMagnitude > 0f) {
            source.audioSource.volume = baseVolume + Mathf.Lerp(minMaxVolume.x, minMaxVolume.y, value);
        }
    }

    protected override void OnInit(object data) {
        base.OnInit(data);
        basePitch = source.audioSource.pitch;
        baseVolume = source.audioSource.volume;
    }

}

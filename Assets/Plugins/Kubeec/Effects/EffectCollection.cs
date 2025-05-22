using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EffectCollection : EffectBase, IStateVariable {

    [SerializeField] bool loop = false;
    [SerializeField] List<EffectElement> effectElements = new();

    public event Action<float> onChangeState;
    float currentState;

    void OnValidate() {
        float maxDuration = duration;
        foreach (EffectElement element in effectElements) {
            if (element == null || element.effect == null) {
                continue;
            }
            float current = element.delay + element.effect.duration;
            if (current > maxDuration) {
                maxDuration = current;
            }
        }
        duration = maxDuration;
    }

    public override bool IsPlaying() {
        return base.IsPlaying() && effectElements.Any(x => x.effect.IsPlaying());
    }

    protected override void OnPlay() {
        foreach (EffectElement element in effectElements) {
            this.InvokeDelay(element.effect.Play, element.delay);
        }
    }

    protected override void OnStart() {
        foreach (EffectElement element in effectElements) {
            element.effect.Init();
        }
    }

    protected override void OnStop() {
        foreach (EffectElement element in effectElements) {
            element.effect.Stop();
        }
    }

    protected override void OnEndPlay() {
        if (loop) {
            Play();
        }
    }

    public float GetState() {
        return currentState;
    }

    public void SetState(float value) {
        currentState = value;
        foreach (EffectElement element in effectElements) {
            if (element.effect is IStateVariable variable) {
                variable.SetState(currentState);
            }
        }
        onChangeState?.Invoke(currentState);
    }

    [System.Serializable]
    public class EffectElement {
        public EffectBase effect;
        public float delay = 0f;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectCollection : EffectBase {

    [SerializeField] List<EffectElement> effectElements = new();

    void OnValidate() {
        float maxDuration = duration;
        foreach (EffectElement element in effectElements) {
            float current = element.delay + element.effect.Duration;
            if (current > maxDuration) {
                maxDuration = current;
            }
        }
        duration = maxDuration;
    }

    protected override void OnPlay() {
        foreach (EffectElement element in effectElements) {
            this.InvokeDelay(element.effect.Play, element.delay);
        }
    }

    protected override void OnStart() {
    }


    [System.Serializable]
    public class EffectElement {
        public EffectBase effect;
        public float delay = 0f;
    }

}

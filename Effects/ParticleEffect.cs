using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEffect : EffectBase {

    [SerializeField] ParticleSystem particleSystem;

    void Reset() {
        particleSystem = GetComponent<ParticleSystem>();
    }

    void OnValidate() {
        if (particleSystem != null && particleSystem.main.loop == false) {
            duration = particleSystem.main.duration + particleSystem.main.startDelay.constant;
        }
    }

    public override bool IsPlaying() => base.IsPlaying() && particleSystem.isPlaying;

    protected override void OnStart() {
        particleSystem.Stop();
    }

    protected override void OnPlay() {
        particleSystem.Play();
    }

    protected override void OnStop() {
        particleSystem.Stop();
    }

}

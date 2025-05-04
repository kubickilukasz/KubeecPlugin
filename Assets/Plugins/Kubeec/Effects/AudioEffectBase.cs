using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioEffectBase : EffectBase, IAudio {

    [SerializeField] protected AudioSource source;

    void Reset() {
        source = GetComponent<AudioSource>();
    }

    public void Stop() {
        source.Stop();
    }

    protected override void OnPlay() {
        source.Play();
    }

    protected override void OnStart() {
        Stop();
    }

}

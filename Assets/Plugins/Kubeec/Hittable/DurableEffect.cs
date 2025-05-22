using System.Collections.Generic;
using UnityEngine;

namespace Kubeec.Hittable {

    public class DurableEffect : EffectBase {

        [SerializeField] ParticleEffect particleEffect;
        [SerializeField] AudioEffectBase audioEffect;
        [SerializeField] GameObject resizableObject;

        [Space]

        [SerializeField] Vector3 minSize;
        [SerializeField] Vector3 maxSize;
        [SerializeField] float minFrequency;
        [SerializeField] float maxFrequency;

        public float deepValue { get; set; } = 0f;

        public void SetDeep(float value) {
            deepValue = value;
            float lValue = Mathf.Log(value + 1f);
            duration = audioEffect.duration = particleEffect.duration = Mathf.LerpUnclamped(minFrequency, maxFrequency, Mathf.Max(0f,1f - lValue));
            resizableObject.transform.localScale = Vector3.LerpUnclamped(minSize, maxSize, Mathf.Max(lValue, 0f));
        }

        protected override void OnPlay() {
            particleEffect.Play();
            audioEffect.Play();
        }

        protected override void OnStart() {
            SetDeep(0f);
            destroyOnEnd = false;
            particleEffect.playOnSpawn = audioEffect.playOnSpawn = false;
            particleEffect.destroyOnEnd = audioEffect.destroyOnEnd = false;
            duration = Mathf.Max(audioEffect.duration, particleEffect.duration);
        }

        protected override void OnEndPlay() {
            Play();
        }

    }

}

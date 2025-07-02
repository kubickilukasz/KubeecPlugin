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
            value = Mathf.Log(value * 0.1f + 1) * 8f;
            duration = audioEffect.duration = particleEffect.duration = Mathf.Lerp(minFrequency, maxFrequency, 1f - value);
            resizableObject.transform.localScale = minSize * value;
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

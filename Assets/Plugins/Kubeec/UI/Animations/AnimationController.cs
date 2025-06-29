using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UI {

    public class AnimationController : EnableDisableRectInitableDisposable, IAnimation{

        [SerializeField] List<AnimationBase> animations = new();

        float maxDurationPlay, maxDurationPlayBackwards;
        Coroutine coroutine;

        [Button]
        void Reset() {
            animations = GetComponentsInChildren<AnimationBase>(true).ToList();
        }

        protected override void OnInit(object data = null) {
            animations.ForEach(anim => anim.Init());
            maxDurationPlay = animations.Max(x => x.GetPlayDuration());
            maxDurationPlayBackwards = animations.Max(x => x.GetPlayBackwardsDuration());
        }

        protected override void OnDispose() {
            Stop();
        }

        public void Pause() {
            animations.ForEach(t => t.Pause());
        }

        [Button]
        public void Play(float? duration = null, Action onComplete = null) {
            float maxDuration = Mathf.Max(maxDurationPlay, duration ?? 0);
            animations.ForEach(t => t.Play(duration));
            if (coroutine != null) {
                StopCoroutine(coroutine);
            }
            coroutine = StartCoroutine(WaitAndCall(maxDuration, onComplete));
        }

        [Button]
        public void PlayBackwards(float? duration = null, Action onComplete = null) {
            float maxDuration = Mathf.Max(maxDurationPlayBackwards, duration ?? 0);
            animations.ForEach(t => t.PlayBackwards(duration));
            if (coroutine != null) {
                StopCoroutine(coroutine);
            }
            coroutine = StartCoroutine(WaitAndCall(maxDuration, onComplete));
        }

        public void Stop() {
            animations.ForEach(t => t.Stop());
        }

        public void ResetToPlay() {
            animations.ForEach(t => t.ResetToPlay());
        }

        public void ResetToPlayBackwards() {
            animations.ForEach(t => t.ResetToPlayBackwards());
        }

        IEnumerator WaitAndCall(float time, Action onComplete) {
            yield return new WaitForSeconds(time);
            onComplete?.Invoke();
        }

    }

}

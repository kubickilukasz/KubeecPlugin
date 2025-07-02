using System;
using UnityEngine;
using DG.Tweening;

namespace UI {

    [RequireComponent(typeof(CanvasGroup))]
    public class CanvasFadeAnimation : AnimationBase {

        [SerializeField] float duration = 0.5f;

        CanvasGroup canvasGroup;
        Sequence sequence;

        protected override void OnInit(object boj = null) {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public override void Pause() {
            if (sequence != null) {
                sequence.Pause();
            }
        }

        public override void Play(float? duration = null, Action onComplete = null) {
            if (!IsInitialized()) {
                onComplete?.Invoke();
                return;
            }
            Stop();
            duration ??= this.duration;
            sequence = DOTween.Sequence();
            sequence.Append(canvasGroup.DOFade(1f, duration.Value));
            sequence.OnComplete(() => {
                canvasGroup.interactable = canvasGroup.blocksRaycasts = true;
                onComplete?.Invoke();
            });
            sequence.Play();
        }

        public override void PlayBackwards(float? duration = null, Action onComplete = null) {
            if (!IsInitialized()) {
                onComplete?.Invoke();
                return;
            }
            Stop();
            duration ??= this.duration;
            sequence = DOTween.Sequence();
            canvasGroup.interactable = canvasGroup.blocksRaycasts = false;
            sequence.Append(canvasGroup.DOFade(0f, duration.Value));
            sequence.OnComplete(() => { 
                onComplete?.Invoke(); 
            });
            sequence.Play();
        }

        public override void Stop() {
            if (sequence != null) {
                sequence.Complete();
                sequence = null;
            }
        }

        public override void ResetToPlay() {
            Play();
            Stop();
        }

        public override void ResetToPlayBackwards() {
            PlayBackwards();
            Stop();
        }

        public override float GetPlayDuration() {
            return duration;
        }

        public override float GetPlayBackwardsDuration() {
            return duration;
        }
    }

}

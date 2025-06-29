using System;
using UnityEngine;

namespace UI {

    public class BlankAnimation : AnimationBase {

        public override float GetPlayBackwardsDuration() {
            return 0f;
        }

        public override float GetPlayDuration() {
            return 0f;
        }

        public override void Pause() {
        }

        public override void Play(float? duration = null, Action onComplete = null) {
        }

        public override void PlayBackwards(float? duration = null, Action onComplete = null) {
        }

        public override void ResetToPlay() {
        }

        public override void ResetToPlayBackwards() {
        }

        public override void Stop() {
        }
    }

}

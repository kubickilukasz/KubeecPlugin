using UnityEngine;
using System;

namespace UI {

    public abstract class AnimationBase : RectMonoBehaviour, IAnimation {

        public abstract void Pause();

        public abstract void Play(float? duration = null, Action onComplete = null);

        public abstract void PlayBackwards(float? duration = null, Action onComplete = null);

        public abstract void Stop();

        public virtual void ResetToPlay() {
            Play(0f);
        }

        public virtual void PlayBackwards() {
            PlayBackwards(0f);
        }

    }

}

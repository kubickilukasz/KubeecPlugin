using UnityEngine;
using System;

namespace UI {

    public abstract class AnimationBase : EnableDisableRectInitableDisposable, IAnimation{

        public abstract float GetPlayDuration();

        public abstract float GetPlayBackwardsDuration();

        public abstract void Pause();

        public abstract void Play(float? duration = null, Action onComplete = null);

        public abstract void PlayBackwards(float? duration = null, Action onComplete = null);

        public abstract void Stop();

        public abstract void ResetToPlay();

        public abstract void ResetToPlayBackwards();

    }

}

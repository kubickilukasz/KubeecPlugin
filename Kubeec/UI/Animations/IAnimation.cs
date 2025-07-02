using UnityEngine;
using System;

namespace UI {

    public interface IAnimation {

        public void Play(float? duration = null, Action onComplete = null);

        public void PlayBackwards(float? duration = null, Action onComplete = null);

        public void Stop();

        public void Pause();

        public void ResetToPlay();

        public void ResetToPlayBackwards();

    }

}

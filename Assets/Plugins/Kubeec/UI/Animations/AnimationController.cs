using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UI {

    public class AnimationController : RectMonoBehaviour, IAnimation {

        [SerializeField] List<AnimationBase> animations = new();

        [Button]
        void Reset() {
            animations = GetComponentsInChildren<AnimationBase>(true).ToList();
        }

        public void Pause() {
            animations.ForEach(t => t.Pause());
        }

        [Button]
        public void Play(float? duration = null, Action onComplete = null) {
            animations.ForEach(t => t.Play(duration, onComplete));
        }

        [Button]
        public void PlayBackwards(float? duration = null, Action onComplete = null) {
            animations.ForEach(t => t.PlayBackwards(duration, onComplete));
        }

        public void Stop() {
            animations.ForEach(t => t.Stop());
        }
    }

}

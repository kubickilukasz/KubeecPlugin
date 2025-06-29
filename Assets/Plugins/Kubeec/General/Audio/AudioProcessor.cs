using System;
using UnityEngine;

public class AudioProcessor : EnableDisableInitableDisposable, IAudio {

    [SerializeField] AudioReference audioReference;
    [SerializeField] bool playOnInit = false;

    [Space]

    [SerializeField] Vector2 minMaxPitch;
    [SerializeField] Vector2 minMaxVolume;

    AudioObject audioObject;
    float startPitch;
    float startVolume;

    public void Play() {
        if (IsInitialized()) {
            audioObject.Play();
        }
    }

    public void Stop() {
        if (IsInitialized()) {
            audioObject.Stop();
        }
    }

    public void SetPitch(float t) {
        if (IsInitialized()) {
            audioObject.audioSource.pitch = Mathf.Lerp(minMaxPitch.x, minMaxPitch.y, t);
        }
    }

    public void SetPitch(float time, Action onComplete, float min = 0f, float max = 1f) {
        if (IsInitialized()) {
            Timer.Start(time, onComplete, t => {
                if (this != null){
                    t = Mathf.Lerp(min, max, t /= time);
                    audioObject.audioSource.pitch = Mathf.Lerp(minMaxPitch.x, minMaxPitch.y, t);
                }
            });
           
        }
    }

    public void SetVolume(float t) {
        if (IsInitialized()) {
            audioObject.audioSource.volume = Mathf.Lerp(minMaxVolume.x, minMaxVolume.y, t);
        }
    }

    public void SetVolume(float time, Action onComplete, float min = 0f, float max = 1f) {
        if (IsInitialized()) {
            Timer.Start(time, onComplete, t => {
                if (this != null) {
                    t = Mathf.Lerp(min, max, t /= time);
                    audioObject.audioSource.volume = Mathf.Lerp(minMaxVolume.x, minMaxVolume.y, t);
                }
            });

        }
    }

    protected override void OnInit(object data) {
        if (audioReference) {
            startPitch = audioReference.audioObject.audioSource.pitch;
            startVolume = audioReference.audioObject.audioSource.volume;
            audioObject = audioReference.Get(transform);
            if (playOnInit) {
                Play();
            }
        } else {
            Dispose();
        }
    }

    protected override void OnDispose() {
        if (audioObject && this != null) {
            audioObject.Stop();
            this.SafeInvokeNextFrame(() => {
                audioReference.Release(audioObject);
                audioObject = null;
            });
        }
    }
}

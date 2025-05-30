using Kubeec.General;
using UnityEngine;
using UnityEngine.Audio;

public class AudioObject : InitableDisposable<IAudioReference>, IAudio {

    const float minWaitForSound = 0.1f;

    [SerializeField] AudioSource _audioSource;

    IAudioReference currentAudioReference;
    AudioResource currentAudioResource;
    float timer = 0f;
    
    public float Timer => timer;
    public bool IsPlaying => _audioSource && _audioSource.isPlaying;

    public AudioSource audioSource {
        get {
            if (_audioSource == null) {
                _audioSource = this.GetOrAdd<AudioSource>();
                _audioSource.playOnAwake = false;
                _audioSource.Stop();
            }
            return _audioSource;
        }
    }

    void Update() {
        if (IsInitialized() && IsPlaying) {
            timer += Time.unscaledDeltaTime;
        }
    }

    public void Play() {
        if (IsInitialized()) {
            float minTimer = currentAudioReference.GetMinTimer();
            if (minTimer < minWaitForSound) {
                this.InvokeDelay(Play, minWaitForSound);
                return;
            }
            timer = 0f;
            currentAudioReference.Register(this);
            audioSource.Play();
        }
    }

    public void Stop() {
        if (IsInitialized()) {
            currentAudioReference.Unregister(this);
            audioSource.Stop();
            timer = 0f;
        }
    }

    protected override void OnInit(IAudioReference data) {
        currentAudioReference = data;
        timer = 0f;
        if (currentAudioResource == null) {
            currentAudioResource = audioSource.resource;
        }
        if (data != null) {
            audioSource.resource = data.audioResource;
        } else {
            audioSource.resource = currentAudioResource;
        }
    }

    protected override void OnDispose() {
        currentAudioReference.Unregister(this);
        if (_audioSource != null) {
            _audioSource.Stop();
        }
    }

}

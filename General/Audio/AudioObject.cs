using Kubeec.General;
using UnityEngine;
using UnityEngine.Audio;

public class AudioObject : InitableDisposable<IAudioReference>, IAudio {

    const float minWaitForSound = 0.05f;

    [SerializeField] AudioSource _audioSource;
    [SerializeField] bool useRandomPitch = false;

    IAudioReference currentAudioReference;
    Coroutine coroutine;
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
        if (IsInitialized() && timer < minWaitForSound) {
            timer += Time.unscaledDeltaTime;
        }
    }

    public void Play() {
        if (IsInitialized()) {
            float minTimer = currentAudioReference.GetMinTimer();
            if (coroutine != null) {
                StopCoroutine(coroutine);
                coroutine = null;
            }
            if (minTimer < minWaitForSound) {
                if (!currentAudioReference.PlayOnce) {
                    this.InvokeDelay(Play, minWaitForSound - minTimer);
                }
                return;
            }
            timer = 0f;
            currentAudioReference.Register(this);
            if (useRandomPitch) {
                audioSource.pitch = RandomPitch(0.9f);
            }
            audioSource.Play();
        }
    }

    public void Stop() {
        if (IsInitialized()) {
            if (coroutine != null) {
                StopCoroutine(coroutine);
                coroutine = null;
            }
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

    // https://www.reddit.com/r/Unity3D/comments/18ycc02/comment/kgaoj15/
    float RandomPitch(float pitch) {
        int[] pentatonicSemitones = new[] { 0, 2, 4, 7, 9 };
        int index = Random.Range(0, pentatonicSemitones.Length);
        int x = pentatonicSemitones[index];
        for (int i = 0; i < x; i++) {
            pitch *= 1.059463f;
        }
        return pitch;
    }

}

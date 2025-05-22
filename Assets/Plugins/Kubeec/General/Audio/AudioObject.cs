using Kubeec.General;
using UnityEngine;
using UnityEngine.Audio;

public class AudioObject : Initable<AudioResource>, IAudio {

    [SerializeField] AudioSource _audioSource;

    AudioResource currentAudioResource;

    public AudioSource audioSource {
        get {
            if (_audioSource == null) {
                _audioSource = this.GetOrAdd<AudioSource>();
            }
            return _audioSource;
        }
    }

    public void Play() {
        if (IsInitialized()) {
            audioSource.Play();
        }
    }

    public void Stop() {
        if (IsInitialized()) {
            audioSource.Stop();
        }
    }

    protected override void OnInit(AudioResource data) {
        if (currentAudioResource == null) {
            currentAudioResource = audioSource.resource;
        }
        if (data != null) {
            audioSource.resource = data;
        } else {
            audioSource.resource = currentAudioResource;
        }
    }

}

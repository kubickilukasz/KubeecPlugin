using UnityEngine;
using UnityEngine.Audio;
using Kubeec.General;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
[CreateAssetMenu(fileName = "AudioReference", menuName = "ScriptableObjects/AudioReference")]
public class AudioReference : ScriptableObject, IAudioReference {

    [SerializeField] AudioObject _audioObject;
    [SerializeField] AudioResource _audioResource;
    [SerializeField] bool playOnce = false;
    [SerializeField] float volumeMultiplier = 1.0f;

    public AudioObject audioObject => _audioObject;
    public AudioResource audioResource => _audioResource;
    public bool PlayOnce => playOnce;

    HashSet<AudioObject> registeredObjects = new HashSet<AudioObject>();

    public void Register(AudioObject obj) {
        if (!registeredObjects.Contains(obj)) {
            obj.audioSource.volume *= volumeMultiplier;
            registeredObjects.Add(obj);
        }
    }

    public void Unregister(AudioObject obj) {
        if (registeredObjects.Contains(obj)) {
            registeredObjects.Remove(obj);
        }
    }

    public float GetMinTimer() {
        if (registeredObjects == null || registeredObjects.Count == 0) {
            return float.MaxValue;
        }
        return registeredObjects.Min(x => x.IsPlaying ? x.Timer : float.MaxValue);
    }

}

public interface IAudioReference {

    public AudioObject audioObject { get; }
    public AudioResource audioResource { get; }
    public bool PlayOnce { get; }

    public void Register(AudioObject obj);
    public void Unregister(AudioObject obj);
    public float GetMinTimer();

}

public static class AudioReferenceExt {

    public static AudioObject Get(this IAudioReference audioReference, Transform parent = null) {
        return AudioPool.Get(audioReference, parent);
    }

    public static void Release(this IAudioReference audioReference, AudioObject audioObject) {
        audioObject.Dispose();
        AudioPool.Release(audioReference, audioObject);
    }

}

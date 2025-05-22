using UnityEngine;
using UnityEngine.Audio;
using Kubeec.General;

[System.Serializable]
[CreateAssetMenu(fileName = "AudioReference", menuName = "ScriptableObjects/AudioReference")]
public class AudioReference : ScriptableObject, IAudioReference {

    [SerializeField] AudioObject _audioObject;
    [SerializeField] AudioResource _audioResource;

    public AudioObject audioObject => _audioObject;
    public AudioResource audioResource => _audioResource;

}

public interface IAudioReference {

    public AudioObject audioObject { get; }
    public AudioResource audioResource { get; }

}

public static class AudioReferenceExt {

    public static AudioObject Get(this IAudioReference audioReference, Transform parent = null) {
        return AudioPool.Get(audioReference, parent);
    }

    public static void Release(this IAudioReference audioReference, AudioObject audioObject) {
        AudioPool.Release(audioReference, audioObject);
    }

}

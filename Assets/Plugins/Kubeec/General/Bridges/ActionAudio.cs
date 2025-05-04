using UnityEngine;

[RequireComponent(typeof(IAction), typeof(AudioSource))]
public class ActionAudio : EnableDisableInitableDisposable, IAudio{

    IAction action;
    AudioSource audioSource;

    public void Play() {
        if (!IsInitialized()) {
            return;
        }
        audioSource.Play();
    }

    public void Stop() {
        if (!IsInitialized()) {
            return;
        }
        audioSource.Stop();
    }

    protected override void OnInit(object data) {
        action = GetComponent<IAction>();
        audioSource = GetComponent<AudioSource>();
        action.onAction += Play;
    }

    protected override void OnDispose() {
        if (action != null) {
            action.onAction -= Play;
        }
    }

}

using UnityEngine;

public abstract class AudioEndpoint : EnableDisableInitableDisposable, IAudio {

    [SerializeField] AudioReference audioReference;

    protected AudioObject _source;
    protected AudioObject source {
        get {
            if (_source == null) {
                _source = audioReference.Get(transform);
                _source.transform.localPosition = Vector3.zero;
            }
            return _source;
        }
    }

    public void Play() {
        if (!IsInitialized()) {
            return;
        }
        source.Play();
    }

    public void Stop() {
        if (!IsInitialized()) {
            return;
        }
        if (_source != null) {
            _source.Stop();
            audioReference.Release(_source);
            _source = null;
        }
    }

    protected override void OnDispose() {
        if (_source != null) {
            this.SafeInvokeNextFrame(() => {
                audioReference.Release(_source);
                _source = null;
            });
        }
    }
}

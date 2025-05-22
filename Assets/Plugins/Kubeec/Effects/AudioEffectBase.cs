using System.Collections;
using UnityEngine;

public class AudioEffectBase : EffectBase, IAudio {

    [SerializeField] protected AudioReference prefab;

    protected AudioObject _source;
    protected AudioObject source {
        get {
            if (_source == null) {
                _source = prefab.Get(transform.parent);
                _source.transform.localPosition = Vector3.zero;
            }
            return _source;
        }
    }

    public override bool IsPlaying() {
        return base.IsPlaying() && _source != null && _source.audioSource.isPlaying;
    }

    protected override void OnInit(object data) {
        base.OnInit(data);
    }

    protected override void OnDispose() {
        if (_source != null) {
            prefab.Release(_source);
            _source = null;
        }
        base.OnDispose();
    }

    protected override void OnStop() {
        source.Stop();
    }

    protected override void OnPlay() {
        source.Play();
    }

    protected override void OnStart() {
        Stop();
    }

}

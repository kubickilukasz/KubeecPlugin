using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateTransferer : EnableDisableInitableDisposable{

    [SerializeField] bool onUpdate = false;
    [SerializeField] MonoBehaviour from;
    [SerializeField] MonoBehaviour[] to;

    IStateFloat _from;
    IStateFloat [] _to;

    void OnValidate() {
        if (from.gameObject.TryGetComponent(out _from)) {
            from = _from as MonoBehaviour;
        } else {
            Debug.LogError($"{from.name} is not IStateFloat");
            from = null;
        }
        for (int i = 0; i < to.Length; i++) {
            if (to[i] != null && to[i].gameObject.TryGetComponent(out IStateFloat _to)) {
                to[i] = _to as MonoBehaviour;
            } else {
                Debug.LogError($"{to[i].name} is not IStateFloat");
                to[i] = null;
            }
        }
    }

    void Update() {
        if (IsInitialized() && onUpdate) {
            UpdateState();
        }
    }

    protected override void OnInit(object data) {
        _from = from as IStateFloat;
        _to = new IStateFloat[to.Length];
        for (int i = 0; i < to.Length; i++) {
            _to[i] = to[i] as IStateFloat;
        }
        UpdateState();
        if (!onUpdate) {
            _from.onChangeState += UpdateState;
        }
    }

    protected override void OnDispose() {
        if (!onUpdate && from != null) {
            _from.onChangeState -= UpdateState;
        }
    }

    void UpdateState() {
        UpdateState(_from.GetState());
    }

    void UpdateState(float value) {
        for (int i = 0; i < to.Length; i++) {
            _to[i].SetState(value);
        }
    }

}

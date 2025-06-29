using UnityEngine;

[RequireComponent(typeof(IStateFloat))]
public class StateAudio : AudioEndpoint{

    [SerializeField] [Range(0, 1f)] float threshold = 0.4f;

    IStateFloat stateVariable;
    float currentValue;

    protected override void OnInit(object data) {
        stateVariable = GetComponent<IStateFloat>();
        currentValue = stateVariable.GetState();
        stateVariable.onChangeState += OnChangeState;
    }

    protected override void OnDispose() {
        base.OnDispose();
        if (stateVariable != null) {
            stateVariable.onChangeState -= OnChangeState;
        }
    }

    void OnChangeState(float newValue) {
        float delta = Mathf.Abs(newValue - currentValue);
        if (delta >= threshold) {
            Play();
            currentValue = newValue;
        }
    }

}

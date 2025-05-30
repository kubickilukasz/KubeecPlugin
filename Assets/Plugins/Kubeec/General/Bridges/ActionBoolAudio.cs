using UnityEngine;

[RequireComponent(typeof(IActionBool))]
public class ActionBoolAudio : AudioEndpoint{

    [SerializeField] bool playOnTrue = true;
    [SerializeField] bool playOnFalse = false;
    IActionBool action;

    protected override void OnInit(object data) {
        action = GetComponent<IActionBool>();
        action.onAction += OnAction;
    }

    protected override void OnDispose() {
        base.OnDispose();
        if (action != null) {
            action.onAction -= OnAction;
        }
    }

    void OnAction(bool value) {
        if (value && playOnTrue) {
            Play();
        } else if (!value && playOnFalse) {
            Play();
        } else {
            Stop();
        }
    }

}

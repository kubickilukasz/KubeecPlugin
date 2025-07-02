using UnityEngine;

[RequireComponent(typeof(IAction))]
public class ActionAudio : AudioEndpoint {

    IAction action;

    protected override void OnInit(object data) {
        action = GetComponent<IAction>();
        action.onAction += Play;
    }

    protected override void OnDispose() {
        base.OnDispose(); 
        if (action != null) {
            action.onAction -= Play;
        }
    }

}

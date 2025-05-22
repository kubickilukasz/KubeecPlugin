using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IStateVariable {

    public event Action<float> onChangeState;

    float GetState();

    void SetState(float value);

}

public abstract class IStateReference : MonoBehaviour, IStateVariable {

    public event Action<float> onChangeState;

    public float GetState() {
        throw new NotImplementedException();
    }

    public void SetState(float value) {
        throw new NotImplementedException();
    }
}

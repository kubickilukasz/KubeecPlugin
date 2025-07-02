using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IStateFloat : IStateVariable<float>{

}

public interface IStateVector2 : IStateVariable<Vector2> {


}

public interface IStateVariable<T>{

    public event Action<T> onChangeState;

    T GetState();

    void SetState(T value);

}


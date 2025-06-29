using System;
using UnityEngine;

public interface IAction{

    public event Action onAction;

}

public interface IActionBool{

    public event Action<bool> onActionBool;

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>{

    static T _instance;

    public static T instance {
        get {
            if (_instance == null) {
                _instance = FindAnyObjectByType<T>(FindObjectsInactive.Exclude);
                if (_instance == null) {
                    _instance = FindAnyObjectByType<T>(FindObjectsInactive.Include);
                }
                if (_instance == null) {
                    _instance = new GameObject($"[Generated] {typeof(T).Name}").AddComponent<T>();
                }
                _instance.OnInit();
            }
            return _instance;
        }
    }


    public static bool instanceExist => _instance != null;

    protected virtual void Awake() {
        if (_instance == null) {
            _instance = (T)this;
            _instance.OnInit();
        } else if(_instance != this) {
            DestroyImmediate(this);
        }
    }

    protected virtual void OnInit() { }

}

using Kubeec.General;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour{

    float currentTimer;
    float time;
    Action onComplete;
    Action<float> onUpdate;

    static CustomPool<Timer> customPool = new CustomPool<Timer>(); 

    void Update() {
        currentTimer += Time.deltaTime;
        onUpdate?.Invoke(currentTimer);
        if (currentTimer >= time) {
            onComplete?.Invoke();
            customPool.Release(this);
        }
    }

    public static Timer Start(float time, Action onComplete, Action<float> onUpdate = null) {
        if (customPool == null) {
            customPool = new CustomPool<Timer>();
        }

        Timer timer = customPool.Get();
        timer.Set(time, onComplete);
        timer.onUpdate = onUpdate;
        return timer;
    }

    public void Set(float time, Action onComplete) {
        this.time = time;
        this.onComplete = onComplete;
        currentTimer = 0f;
    }

    public void Stop(bool invokeOnComplete = false) {
        if (invokeOnComplete) {
            onComplete?.Invoke();
        }
        Destroy(gameObject);
    }

}

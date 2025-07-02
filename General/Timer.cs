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
    bool isGoing = false;

    static CustomPool<Timer> customPool = new CustomPool<Timer>(); 

    void Update() {
        currentTimer += Time.deltaTime;
        onUpdate?.Invoke(currentTimer);
        if (currentTimer >= time) {
            isGoing = false;
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
        timer.isGoing = true;
        return timer;
    }

    public void Set(float time, Action onComplete) {
        this.time = time;
        this.onComplete = onComplete;
        currentTimer = 0f;
    }

    public void Stop(bool invokeOnComplete = false) {
        if (isGoing) {
            isGoing = false;
            if (invokeOnComplete) {
                onComplete?.Invoke();
            }
            customPool.Release(this);
        }
    }

}

using UnityEngine;

public class LazySingleton<T> : EnableDisableInitableDisposable where T : LazySingleton<T> {

    static T instance;
    public static T Instance => instance;

    protected override void OnInit(object data) {
        if (instance == null) {
            instance = this as T;
        } else {
            Dispose();
        }
    }

    protected override void OnDispose() {
        if (instance == this) {
            instance = null;
        }
    }

}

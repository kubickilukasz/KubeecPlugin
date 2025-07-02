using Kubeec.General;
using UnityEngine;

public class DestructibleInitableTransferer : InitableTransferer {

    [SerializeField] Destructible destructible;
    [SerializeField] bool destroyOnInit = false;

    protected override void OnTransfererDispose() {
        if (!destroyOnInit) {
            destructible.Destruct();
        }
    }

    protected override void OnTransfererInit() {
        if (destroyOnInit) {
            destructible.Destruct();
        }
    }
}

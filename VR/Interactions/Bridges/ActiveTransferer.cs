using UnityEngine;
using Kubeec.VR.Interactions;

namespace Kubeec.VR {
    public abstract class ActiveTransferer : EnableDisableInitableDisposable {

        [SerializeField] InteractionBase transferFrom;

        protected override void OnInit(object data) {
            transferFrom.onActive += OnActive;
            transferFrom.onInactive += OnInactive;
            Refresh();
        }

        protected override void OnDispose() {
            if (transferFrom) {
                transferFrom.onActive -= OnActive;
                transferFrom.onInactive -= OnInactive;
            }
        }

        protected abstract void OnActive();
        protected abstract void OnInactive();

        protected void Refresh() {
            if (transferFrom.status == InteractionStatus.Active) {
                OnActive();
            } else {
                OnInactive();
            }
        }

    }
}

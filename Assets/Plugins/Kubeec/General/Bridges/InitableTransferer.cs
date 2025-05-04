using System.Collections.Generic;
using UnityEngine;

namespace Kubeec.General {

    public abstract class InitableTransferer : EnableDisableInitableDisposable {

        [SerializeField] InitableDisposable transfererFrom;

        protected override void OnInit(object data) {
            transfererFrom.onInit += OnTransfererInit;
            transfererFrom.onDispose += OnTransfererDispose;
        }

        protected override void OnDispose() {
            if (transfererFrom) {
                transfererFrom.onInit -= OnTransfererInit;
                transfererFrom.onDispose -= OnTransfererDispose;
            }
        }

        protected abstract void OnTransfererInit();
        protected abstract void OnTransfererDispose();

        protected void Refresh() {
            if (transfererFrom.IsInitialized()) {
                OnTransfererInit();
            } else {
                OnTransfererDispose();
            }
        }

    }

}

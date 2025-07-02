using System.Collections.Generic;
using UnityEngine;

namespace Kubeec.VR {

    public class InitableActiveTransferer : ActiveTransferer {

        [SerializeField] List<EnableDisableInitableDisposable> initables = new();

        protected override void OnActive() {
            foreach (var initable in initables) {
                initable.Init();
            }
        }

        protected override void OnInactive() {
            foreach (var initable in initables) {
                initable.Dispose();
            }
        }

    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace Kubeec.VR.Player {

    [DefaultExecutionOrder(10)]
    public class LoadOnStartPlayerSpace : EnableDisableInitableDisposable {

        [SerializeField] PlayerSpace space;
        [SerializeField] Transform startPosition;

        protected override void OnInit(object data) {
            LocalPlayerReference.SafeGetPlayerController(controller => {
                LocalPlayerReference.instance.PlayerController.SetPlayerSpace(space);
                if (startPosition) {
                    LocalPlayerReference.instance.PlayerController.ResetPosition(startPosition.position, startPosition.rotation);
                } else {
                    LocalPlayerReference.instance.PlayerController.ResetPosition();
                }
            });
        }


    }

}

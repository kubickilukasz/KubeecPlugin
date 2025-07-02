using Kubeec.VR.Interactions;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Kubeec.VR.Player {

    public class ViewBelt : EnableDisableInitableDisposable {
        
        [SerializeField] List<InteractionSocket> sockets = new List<InteractionSocket>();
        [SerializeField] Transform start;
        [SerializeField] Transform end;

        protected override void OnInit(object data) {
            foreach (InteractionSocket socket in sockets) {
                socket.onActive += Refresh;
                socket.onInactive += Refresh;
            }
            Refresh();
        }

        protected override void OnDispose() {
            foreach (InteractionSocket socket in sockets) {
                socket.onActive -= Refresh;
                socket.onInactive -= Refresh;
            }
        }

        void Refresh() {
            sockets.Sort((x,y) => x.status == y.status ? 0 : x.status == InteractionStatus.Active ? -1 : 1);
            this.Log(MyDebug.TypeLog.Temporary, sockets.Select(x => x.status == InteractionStatus.Active));
            int countActive = sockets.Count(x => x.status == InteractionStatus.Active);
            if (countActive < sockets.Count) {
                sockets[countActive].CanInteract = true;
                for (int i = countActive + 1; i < sockets.Count; i++) {
                    sockets[i].CanInteract = false;
                }
            }
            countActive = Mathf.Min(sockets.Count, countActive + 1); 
            Vector3 step = (end.position - start.position) / (countActive + 1);
            for (int i = 0; i < countActive; i++) {
                sockets[i].transform.position = start.position + (step * (i + 1));
            }
        }

        
    }

}

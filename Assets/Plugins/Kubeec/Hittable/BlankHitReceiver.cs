using UnityEngine;


namespace Kubeec.Hittable {

    public class BlankHitReceiver : HitReceiver {

        protected override void SendHitToCollector(HitProvider hitProvider, HitType hitType, float damage) {
        }

    }

}

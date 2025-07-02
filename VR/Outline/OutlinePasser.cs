using UnityEngine;
using Kubeec.VR.Interactions;
using Kubeec.VR.Player;

namespace Kubeec.VR.Outline {

    [RequireComponent(typeof(InteractionBase))]
    public class OutlinePasser : MonoBehaviour {

        [SerializeField] bool showOutlineOnActive = true;

        InteractionBase interaction;
        IOutlineable outlineable;
        LocalPlayerReference player;
        bool tempShowOutlineOnActive;

        void OnEnable() {
            if (interaction == null) {
                interaction = GetComponent<InteractionBase>();
            }
            player = LocalPlayerReference.instance;
            if (interaction is IOutlineable outlineable) {
                this.outlineable = outlineable;
            } else {
                return;
            }
            tempShowOutlineOnActive = showOutlineOnActive;
            Refresh();
            if (showOutlineOnActive) {
                interaction.onActive += Outline;
                interaction.onInactive += StopOutline;
            } else {
                interaction.onActive += StopOutline;
                interaction.onInactive += Outline;
            }
        }

        void OnDisable() {
            outlineable = null;
            if (interaction != null) {
                if (tempShowOutlineOnActive) {
                    interaction.onActive -= Outline;
                    interaction.onInactive -= StopOutline;
                } else {
                    interaction.onActive -= StopOutline;
                    interaction.onInactive -= Outline;
                }
            }
        }

        void Refresh() {
            if ((interaction.status == InteractionStatus.Active && tempShowOutlineOnActive) || (interaction.status == InteractionStatus.Inactive && !tempShowOutlineOnActive)) {
                Outline();
            } else {
                StopOutline();
            }
        }

        void Outline() {
            if (outlineable != null) {
                player.OutlineController.StartOutline(outlineable, this);
            }
        }

        void StopOutline() {
            player.OutlineController.ForceStopOutline(this);
        }

    }

}

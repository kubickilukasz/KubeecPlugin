using System.Collections;
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;


namespace Kubeec.VR.Interactions {

    public class Interactor : NetworkBehaviour {

        protected const int minPriority = -64;

        protected int currentPriority = minPriority;
        protected HashSet<InteractionBase> activeInteractions = new HashSet<InteractionBase>();
        protected InteractionBase activeAloneInteraction;

        public virtual bool StartInteract(InteractionBase interaction) {
            if (CanInteract(interaction)) {
                AddInteraction(interaction);
                return true;
            } else {
                return false;
            }
        }

        public void StopInteract(InteractionBase interaction) {
            if (activeInteractions.Contains(interaction)) {
                activeInteractions.Remove(interaction);
                if (interaction.Equals(activeAloneInteraction)) {
                    activeAloneInteraction = null;
                }
                RefreshPriority();
                OnStopInteract(interaction);
            }
        }

        public void Clear() {
            while (activeInteractions.Count > 0) {
                activeInteractions.First().StopInteract(this);
            }
        }

        protected virtual void OnStartInteract(InteractionBase interaction) { }
        protected virtual void OnStopInteract(InteractionBase interaction) { }

        protected bool CanInteract(InteractionBase interaction) {
            if (interaction == null || !IsServer) return false;
            if (interaction.Priority < currentPriority) return false;
            if (!interaction.CanInteractWithOthers() && activeAloneInteraction != null && interaction != activeAloneInteraction) {
                float dis1 = (transform.position - interaction.GetPositionData(this).position).sqrMagnitude;
                float dis2 = (transform.position - activeAloneInteraction.GetPositionData(this).position).sqrMagnitude;
                if (dis1 > dis2) {
                    return false;
                }
            }
            return true;
        }

        void AddInteraction(InteractionBase interaction) {
            currentPriority = Mathf.Max(currentPriority, interaction.Priority);
            bool containInList = false;
            List<InteractionBase> listInteractions = activeInteractions.ToList();
            for (int i = 0; i < activeInteractions.Count;) {
                if (listInteractions[i] == interaction) {
                    containInList = true;
                    i++;
                    continue;
                }
                if (!CanInteract(listInteractions[i])) {
                    InteractionBase ib = listInteractions[i];
                    activeInteractions.Remove(listInteractions[i]);
                    ib.StopInteract(this);
                } else {
                    i++;
                }
            }
            if (!containInList) {
                OnStartInteract(interaction);
                activeInteractions.Add(interaction);
                if (!interaction.CanInteractWithOthers()) {
                    activeAloneInteraction = interaction;
                }
            }
        }

        void RefreshPriority() {
            if (activeInteractions.Count > 0) {
                currentPriority = activeInteractions.Max(x => x.Priority);
            } else {
                currentPriority = minPriority;
            }
        }

    }

}
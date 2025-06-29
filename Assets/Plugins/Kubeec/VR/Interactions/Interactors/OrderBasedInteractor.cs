using Kubeec.VR.Outline;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kubeec.VR.Interactions {

    public abstract class OrderBasedInteractor<T> : Interactor where T : InteractionBase{

        protected T currentInteraction = null;
        protected InteractionPositionData? closestPositionData;
        protected HashSet<T> potentialInteractions = new HashSet<T>();
        protected int maxPriority;

        protected virtual void Update() {
            FindClosestInteraction();
        }

        public void RegisterPotentialInteraction(T interaction) {
            if (!potentialInteractions.Contains(interaction)) {
                potentialInteractions.Add(interaction);
                maxPriority = Mathf.Max(interaction.Priority, maxPriority);
            }
        }

        public void UnregisterPotentialInteraction(T interaction) {
            if (potentialInteractions.Contains(interaction)) {
                potentialInteractions.Remove(interaction);
                maxPriority = potentialInteractions.Count > 0 ? potentialInteractions.Max(x => x.Priority) : minPriority;
            }
        }

        public bool IsClosestInteraction(T interaction) {
            return closestPositionData.HasValue && closestPositionData.Value.interaction == interaction;
        }


        protected override void OnStopInteract(InteractionBase interaction) {
            if (interaction == currentInteraction) {
                currentInteraction = null;
            }
        }

        protected virtual void FindClosestInteraction() {
            closestPositionData = null;
            float distance = float.MaxValue;
            foreach (T other in potentialInteractions) {
                if (!other.CanInteract || maxPriority > other.Priority) {
                    continue;
                }
                InteractionPositionData interactionPositionData = other.GetPositionData(this);
                float dis = (interactionPositionData.position - transform.position).sqrMagnitude;
                if (distance > dis) {
                    distance = dis;
                    closestPositionData = interactionPositionData;
                }
            }
        }

    }

}

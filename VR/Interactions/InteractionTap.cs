using UnityEngine;
using Kubeec.VR.Outline;
using System.Collections.Generic;
using System;

namespace Kubeec.VR.Interactions {

    public class InteractionTap : InteractionTriggerBase<HandInteractor>, IOutlineable, IAction {

        public event Action onAction;

        [SerializeField] List<OutlineObject> outlineObject;
        [SerializeField] float timeToReset = 1f;

        float timer = 0f;
        bool tap;

        public bool IsTap => tap;
        public float NormalizedTimeTap => timer / timeToReset;
        public bool CanOutline() => true;

        void Update() {
            if (timer < timeToReset) {
                timer += Time.deltaTime;
            } else {
                tap = false;
            }
        }

        public IEnumerable<OutlineObject> GetOutlineObjects(Vector3 source) {
            return outlineObject;
        }

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            timer = timeToReset;
        }

        protected override void OnStartInteract(Interactor handler) {
            base.OnStartInteract(handler);
            timer = 0f;
            tap = true;
            onAction?.Invoke();
        }

        protected override void OnStopInteract(Interactor handler) {
            base.OnStopInteract(handler);
        }

    }

}

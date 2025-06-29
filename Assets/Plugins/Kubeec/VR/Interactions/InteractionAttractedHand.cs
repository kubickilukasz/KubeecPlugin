using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Kubeec.VR.Outline;
using Kubeec.VR.Character;

namespace Kubeec.VR.Interactions {

    public abstract class InteractionAttractedHand : InteractionBase, IOutlineable {

        [SerializeField] HandObserver observer;
        [SerializeField] InputInteraction pickUpButton = InputInteraction.Grip;
        [SerializeField] List<OutlineObject> outlineObjects = new();
        [SerializeField] bool dynamicHandlers = false;
        [SerializeField] bool useFreeHandHandlers = false;
        [SerializeField] protected List<HandHandler> placesToHandle;

        protected bool internalCanAttract = false;
        protected HashSet<HandInteractor> potentialHandlers = new HashSet<HandInteractor>();
        protected List<HandInteractor> attractedHandlers = new List<HandInteractor>();
        protected Dictionary<HandInteractor, HandHandler> reservedPlacesToHandle = new Dictionary<HandInteractor, HandHandler>();

        protected Func<HandInteractor, bool> isTryToPickUp;
        protected Func<HandInteractor, bool> isHolding;

        public override bool Repeat => true;
        public override bool CanInteractWithOthers() => false;
        protected virtual bool detractOnTriggerExit => true;

        void FixedUpdate() {
            if (status == InteractionStatus.Active && IsOwnerInHandlers()) {
                OnFixedUpdateAttractedPosition();
            }
        }

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            internalCanAttract = true;
            if (observer) {
                observer.RegisterEnter(StartCheckAttract, this);
                observer.RegisterExit(EndCheckAttract, this);
            }
            DefineActions();
        }

        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();
            internalCanAttract = false;
            if (observer != null) {
                Clear();
                observer.UnregisterEnter(this);
                observer.UnregisterExit(this);
            }
        }

        public override CharacterHandInteraction GetCharacterHand(HandInteractor handler) {
            if (reservedPlacesToHandle.ContainsKey(handler)) {
                var pose = reservedPlacesToHandle[handler].GetHandPose();
                if (pose != null) {
                    return pose;
                }
            }
            return interactionHand;
        }

        public override InteractionPositionData GetPositionData(Interactor interactor) {
            IOutlineable outlineable = this;
            HandHandler handHandler = null;
            Vector3 position = transform.position;
            if (interactor is HandInteractor hand) {
                HandController controller = hand.controller;
                HandHandler found = FindClosest(hand);
                if (found != null) {
                    outlineable = found;
                    handHandler = found;
                    position = found.GetHandPosition(controller.IsRightHand);
                }
            }
            position = !handHandler && dynamicHandlers ? interactor.transform.position : transform.position;
            return new InteractionPositionData(this, position, outlineable, handHandler);
        }

        public virtual void StartCheckAttract(HandInteractor handler, GameObject source) {
            if (internalCanAttract && !potentialHandlers.Contains(handler)) {
                potentialHandlers.Add(handler);
                handler.RegisterPotentialInteraction(this);
            }
        }

        public virtual void EndCheckAttract(HandInteractor handler, GameObject source) {
            if (potentialHandlers.Contains(handler)) {
                OnEndCheckAttract(handler);
            }
        }

        public virtual bool CanOutline() {
            return CanAttract();
        }

        public IEnumerable<OutlineObject> GetOutlineObjects(Vector3 source) {
            return outlineObjects;
        }

        public virtual bool CanAttract() {
            return internalCanAttract && (reservedPlacesToHandle.Count < placesToHandle.Count || dynamicHandlers);
        }

        public virtual bool CanAttract(HandInteractor handler) {
            if (!internalCanAttract || !handler.CanOverridePositionAndRotation(this)) {
                return false;
            }
            if (attractedHandlers.Contains(handler)) {
                if (!isHolding.Invoke(handler)) {
                    return false;
                }
            } else{
                if (!isTryToPickUp.Invoke(handler)) {
                    return false;
                }
                if (!dynamicHandlers && reservedPlacesToHandle.Count >= placesToHandle.Count) {
                    return false;
                }
            }
            return true;
        }

        public bool TryAttract(HandInteractor interactor, HandHandler handHandler) {
            if (potentialHandlers.Contains(interactor)) {
                HandHandler current = FindClosest(interactor, handHandler);
                if (current != null) {
                    Attract(interactor, current);
                    return true;
                }
            }
            return false;
        }

        public bool TryAttract(HandInteractor interactor) {
            InteractionPositionData interactionPositionData = GetPositionData(interactor);
            if (interactionPositionData.handHandler != null) {
                return TryAttract(interactor, interactionPositionData.handHandler);
            } else {
                return false;
            }
        }

        public bool TryGetHandHandler(HandInteractor hand, out HandHandler handHandler) {
            return reservedPlacesToHandle.TryGetValue(hand, out handHandler);
        }

        protected bool IsOwnerInHandlers() {
            if (status == InteractionStatus.Active) {
                foreach (HandInteractor handler in attractedHandlers) {
                    if (handler.IsOwner) {
                        return true;
                    }
                }
            }
            return false;
        }

        protected virtual void OnAttract(HandInteractor handler, HandHandler snapped) {
            StartInteract(handler);
        }

        protected virtual void OnDetract(HandInteractor handler) {
            StopInteract(handler);
        }

        protected virtual void OnAttractedUpdate() {
        }

        protected virtual void OnFixedUpdateAttractedPosition() {
        }

        protected override void OnUpdate() {
            for (int j = 0; j < attractedHandlers.Count;) {
                HandInteractor handler = attractedHandlers[j];
                if (!CanAttract(handler)) {
                    Detract(handler);
                } else {
                    j++;
                }
            }
            if (attractedHandlers.Count > 0) {
                OnAttractedUpdate();
            }
        }

        protected HandHandler FindClosest(HandInteractor handler, HandHandler potentialHandHandler) {
            if (reservedPlacesToHandle.ContainsKey(handler)) {
                return reservedPlacesToHandle[handler];
            }
            if (reservedPlacesToHandle.Count >= placesToHandle.Count && !dynamicHandlers) {
                return null;
            }
            if (potentialHandHandler != null && placesToHandle.Contains(potentialHandHandler) 
                && !reservedPlacesToHandle.ContainsValue(potentialHandHandler)) {
                reservedPlacesToHandle.Add(handler, potentialHandHandler);
                return potentialHandHandler;
            }
            HandHandler current = FindClosest(handler);
            if (current != null) {
                reservedPlacesToHandle.Add(handler, current);
            } else if (dynamicHandlers) {
                current = CreateDynamicHandler(handler);
                reservedPlacesToHandle.Add(handler, current);
            }
            return current;
        }

        protected HandHandler FindClosest(HandInteractor handler) {
            HandHandler current = null;
            float distance = float.MaxValue;
            foreach (HandHandler t in placesToHandle) {
                if (reservedPlacesToHandle.ContainsValue(t)) {
                    continue;
                }
                if (!t.CanUse(handler)) {
                    continue;
                }
                float d = (handler.controller.GetInputHandPosition() - t.GetHandPosition(handler.controller.IsRightHand)).sqrMagnitude;
                if (d < distance) {
                    distance = d;
                    current = t;
                }
            }
            return current;
        }

        protected void Clear() {
            while (reservedPlacesToHandle.Count > 0) {
                Detract(reservedPlacesToHandle.Keys.First());
            }
            foreach (HandInteractor interactionHand in potentialHandlers) {
                interactionHand.UnregisterPotentialInteraction(this);
            }
            potentialHandlers.Clear();
        }

        protected virtual void DefineActions() {
            pickUpButton.GetInputs(out isTryToPickUp, out isHolding, out _);
        }

        protected virtual void OnEndCheckAttract(HandInteractor handler) {
            potentialHandlers.Remove(handler);
            handler.UnregisterPotentialInteraction(this);
            if (detractOnTriggerExit) {
                if (attractedHandlers.Contains(handler)) {
                    Detract(handler);
                }
            }
        }

        protected void Attract(HandInteractor handler, HandHandler handSetup) {
            if (!attractedHandlers.Contains(handler)) {
                attractedHandlers.Add(handler);
                OnAttract(handler, handSetup);
            }
        }

        protected void Detract(HandInteractor handler) {
            if (attractedHandlers.Contains(handler)) {
                attractedHandlers.Remove(handler);
                OnDetract(handler);
            }
            if (reservedPlacesToHandle.ContainsKey(handler)) {
                if (dynamicHandlers && !placesToHandle.Contains(reservedPlacesToHandle[handler])) {
                    if (useFreeHandHandlers) {
                        HandHandler.RemoveHandHandler((FreeHandHandler)reservedPlacesToHandle[handler]);
                    } else {
                        HandHandler.RemoveHandHandler((StaticHandHandler)reservedPlacesToHandle[handler]);
                    }
                }
                reservedPlacesToHandle.Remove(handler);
            }
        }

        HandHandler CreateDynamicHandler(HandInteractor handler) {
            if (useFreeHandHandlers) {
                return HandHandler.CreateDynamicHandHandler<FreeHandHandler>(handler, transform);
            } else {
                return HandHandler.CreateDynamicHandHandler<StaticHandHandler>(handler, transform);
            }
        }
        
    }

}

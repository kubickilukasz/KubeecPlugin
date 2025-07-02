using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Kubeec.VR.Player;
using Unity.Netcode.Components;
using NaughtyAttributes;

namespace Kubeec.VR.Interactions {

    public class InteractionItem : InteractionAttractedHand, IMoveable, IDestructible {

        const RigidbodyInterpolation defaultInterpolation = RigidbodyInterpolation.Interpolate;
        const RigidbodyInterpolation pickedInterpolation = RigidbodyInterpolation.Interpolate;

        const float maxDistanceToForce = 1f;

        public event Action<InteractionItem> onCriticalForce;

        [SerializeField] bool useWeight = false;
        [SerializeField] [Range(0,1f)] float weight = 0f;
        [SerializeField] float criticalForce = 0.2f;
        [SerializeField] bool detractOnCriticalForce = true;
        [SerializeField] bool resetTransformOnPickUp = false;

        [Space]

        [SerializeField] Collider[] disabledCollidersOnPickUp = new Collider[0];
        [SerializeField] Collider[] disabledCollidersOnActive= new Collider[0];
        [SerializeField] Collider[] enableCollidersOnActive= new Collider[0];
        [SerializeField] Collider[] collisionColliders = new Collider[0];
        [SerializeField] SocketReference[] socketReferences = new SocketReference[0];
        [SerializeField] MeshFilter meshFilter;
        [SerializeField] Rigidbody rigidbody;
        [SerializeField] Interactor interactionHandler;
        [SerializeField] NetworkTransform networkTransform;

        public InteractionSocket socket { get; set; }

        Vector3 targetPosition = Vector3.zero;
        Vector3 targetCenterMass = Vector3.zero;
        Quaternion targetRotation = Quaternion.identity;
        Vector3 previousPosition = Vector3.zero;
        bool needToOverridePosAndRot = false;
        Vector3 positionToOverride;
        Quaternion rotationToOverride;
        float sqrCriticalForce;

        public override bool CanInteractWithOthers() => false;
        public bool IsHeld => status == InteractionStatus.Active;
        public bool IsInSocket => socket != null;
        public bool IsSnapped => IsInSocket || IsHeld;
        public Interactor Interactior => interactionHandler;
        public Rigidbody Rigidbody => rigidbody;
        public SocketReference[] SocketReferences => socketReferences;
        public MeshFilter MeshFilter => meshFilter;
        public NetworkTransform NetworkTransform {
            get {
                if (networkTransform == null) {
                    networkTransform = GetComponent<NetworkTransform>();
                }
                return networkTransform;
            }
        }
        protected override bool detractOnTriggerExit => false;

#if UNITY_EDITOR
        [Button]
        void GetColliders() {
            Collider[] collider = GetComponentsInChildren<Collider>(true);
            List<Collider> temp = new List<Collider>();
            foreach (Collider col in collider) {
                if (!col.isTrigger) {
                    temp.Add(col);
                }
            }
            collisionColliders = temp.ToArray();
        }

        [Button]
        void GetSocketMesh() {
            MeshFilter[] temp = GetComponentsInChildren<MeshFilter>(true);
            foreach (var t in temp) {
                if (t.GetComponentInParent<MeshFilter>() == this) {
                    meshFilter = t;
                    break;
                }
            }
        }
#endif


        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            sqrCriticalForce = criticalForce * criticalForce;
            rigidbody.interpolation = defaultInterpolation;
            RefreshColliders(false);
        }

        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();
        }

        public bool CanMove() {
            return !IsHeld && CanAttract() && potentialHandlers.Count == 0 && !IsInSocket;
        }

        public bool PerformOnCloseAction(HandInteractor hand) {
            return false;
            //return TryAttract(hand);
        }

        public override bool CanAttract() {
            return base.CanAttract() && (!IsInSocket || (IsInSocket && socket.CanInteract));
        }

        public override bool CanOutline() {
            return CanAttract() || CanMove();    
        }

        public void SetCollisionColliders(bool enabled) {
            foreach (Collider item in collisionColliders) {
                item.enabled = enabled;
            }
        }
       
        public Rigidbody GetRigidbody() {
            return rigidbody;
        }

        public void Destruct() {
            if (this != null && NetworkObject != null) {
                Clear();
                interactionHandler?.Clear();
                if (IsServer && IsSpawned) {
                    NetworkObject.Despawn(true);
                }
            }
        }

        protected override void OnFixedUpdateAttractedPosition() {
            CalculatePreviousPosition();
            if (CountHandlers > 1) {
                CalculatePositionItem(out targetPosition, out targetRotation, out targetCenterMass);
            } else if (GetHandlers().First() is HandInteractor hand) {
                CalculatePositionItem(hand, out targetPosition, out targetRotation, out float weight, out targetCenterMass);
            }
            if (useWeight) {
                rigidbody.TryMoveToSmooth(targetPosition, targetRotation, weight);
            } else {
                rigidbody.TryMoveTo(targetPosition, targetRotation, Time.fixedDeltaTime);
            }
        }

        protected override void OnAttractedUpdate() {
            CalculatePositionOfHands();
        }

        protected override void OnStartInteract(Interactor handler) {
            if (CountHandlers == 1) {
                RefreshColliders(true);
                OnPickUp(handler as HandInteractor);
            }
        }

        protected override void OnStopInteract(Interactor handler) {
            HandInteractor hand = handler as HandInteractor;
            hand.StopOverridePositionAndRotation(this);
            if (GetHandlers().Count() == 0) {
                RefreshColliders(false);
                OnDrop(hand);
            }
        }

        void CalculatePositionItem(out Vector3 outputPosition, out Quaternion outputRotation, out Vector3 centerMass) {
            outputPosition = Vector3.zero;
            centerMass = Vector3.zero;
            int i = 0;
            float iWeight = 0f;
            Quaternion[] quaternions = new Quaternion[CountHandlers];
            float[] weights = new float[CountHandlers];
            foreach (HandInteractor handler in GetHandlers()) {
                CalculatePositionItem(handler, out Vector3 pos, out Quaternion rot, out float weight, out Vector3 center);
                outputPosition += pos * weight;
                centerMass += center * weight;
                quaternions[i] = rot;
                weights[i] = weight;
                i++;
                iWeight += weight;
            }
            if (iWeight > 0f) {
                outputPosition /= iWeight;
                outputRotation = MyMath.AverageQuaternion(quaternions, weights);
            } else {
                outputPosition /= i;
                outputRotation = MyMath.AverageQuaternion(quaternions, weights);
            }
        }

        void CalculatePositionItem(HandInteractor handler, out Vector3 outputPosition, out Quaternion outputRotation, out float weight, out Vector3 centerMass) {
            HandHandler snapped = reservedPlacesToHandle[handler];
            HandHandlerData data = snapped.Get(transform, handler);
            outputPosition = data.itemPosition;
            outputRotation = data.itemRotation;
            weight = data.weight;
            centerMass = data.handPosition;
        }

        void CalculatePositionOfHands(bool withCriticalForce = true) {
            float force = 0f;
            List<Interactor> handlers = GetHandlers().ToList();
            foreach (HandInteractor handler in handlers) {
                HandHandler snapped = reservedPlacesToHandle[handler];
                HandHandlerData data = snapped.Get(transform, handler);
                handler.TryOverridePositionAndRotation(this, data.handPosition, data.handRotation);
                float current = handler.controller.GetForceVector().sqrMagnitude;
                force += current;
                if (detractOnCriticalForce && withCriticalForce && current >= sqrCriticalForce) {
                    Detract(handler);
                }
            }
            if (withCriticalForce && force >= sqrCriticalForce && CountHandlers > 1) {
                onCriticalForce?.Invoke(this);
            }
        }

        void CalculatePreviousPosition() {
            previousPosition = targetPosition;
            if (needToOverridePosAndRot) {
                needToOverridePosAndRot = false;
                rigidbody.position = positionToOverride;
                rigidbody.rotation = rotationToOverride;
            }
        }

        void OnPickUp(HandInteractor handler) {
            for (int i = 0; i < disabledCollidersOnPickUp.Length; i++) {
                disabledCollidersOnPickUp[i].enabled = false;
            }
            rigidbody.interpolation = pickedInterpolation;
            if (resetTransformOnPickUp) {
                bool previousKinematic = rigidbody.isKinematic;
                rigidbody.isKinematic = true;
                CalculatePositionItem(out targetPosition, out targetRotation, out targetCenterMass);
                rigidbody.position = targetPosition;
                rigidbody.rotation = targetRotation;
                rigidbody.isKinematic = previousKinematic;
            }
        }

        void OnDrop(HandInteractor handler) {
            for (int i = 0; i < disabledCollidersOnPickUp.Length; i++) {
                disabledCollidersOnPickUp[i].enabled = true;
            }
            rigidbody.interpolation = defaultInterpolation;
            Vector3 force = (targetPosition - previousPosition) * (1f / weight);
            float forceMagnitude = Math.Min(force.magnitude, maxDistanceToForce);
            force = force.normalized * forceMagnitude;
            if (force.sqrMagnitude > float.Epsilon) {
                rigidbody.AddForce((2f * force) / 3f, ForceMode.Impulse);
                rigidbody.AddForceAtPosition(force / 3f, reservedPlacesToHandle[handler].GetHandPosition(handler.controller.IsRightHand), ForceMode.Impulse);
            }
        }

        void RefreshColliders(bool isActive) {
            foreach (Collider c in disabledCollidersOnActive) {
                c.enabled = !isActive;
            }
            foreach (Collider c in enableCollidersOnActive) {
                c.enabled = isActive;
            }
        }

    }

}

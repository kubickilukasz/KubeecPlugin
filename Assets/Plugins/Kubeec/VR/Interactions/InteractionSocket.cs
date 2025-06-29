using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kubeec.VR.Interactions {

    public class InteractionSocket : InteractionBase, IAction {

        public event Action onAction;

        public Action<InteractionItem> onItemAttracted;
        public Action<InteractionItem> onItemDetracted;

        [SerializeField] ItemObserver observer;
        [SerializeField] InteractionItem[] items;
        [SerializeField] SocketReference[] socketReferences;

        [Space]

        [SerializeField] bool disableCollisionDuringHolding = false;
        [SerializeField] Transform snapToTransform;
        //[SerializeField] bool allowOnlyHeldItems = false;

        [Space]

        [SerializeField] bool showGhostOnlyOnTrigger = true;
        [SerializeField] bool showGhostOnlyForLocalPlayer = true;
        [SerializeField] Renderer ghostSocket;

        HashSet<ItemInteractor> potentialItems = new HashSet<ItemInteractor>();

        ItemInteractor currentItemInSocket;
        ItemInteractor currentGhostItem;
        MeshFilter ghostSocketMeshFilter;
        Mesh defaultMesh;
        Transform defaultMeshTransform;
        bool wasFixedUpdate = false;
        bool wasLateUpdate = false;

        public InteractionItem CurrentItemInSocket => currentItemInSocket?.InteractionItem;
        public bool IsItemInSocket => currentItemInSocket != null;

        protected override void OnDisable() {
            base.OnDisable();
            HandleGhost();
        }

        void FixedUpdate() {
            if (!wasFixedUpdate && CanUpdateTransformItem()) {
                CalculatePositionAndRotationItem(out Vector3 position, out Quaternion rotation);
                currentItemInSocket.InteractionItem.Rigidbody.position = position;
                currentItemInSocket.InteractionItem.Rigidbody.rotation = rotation;
                wasFixedUpdate = true;
                wasLateUpdate = false;
            }
        }

        void LateUpdate() {
            if (wasFixedUpdate) {
                if (!wasLateUpdate) {
                    wasLateUpdate = true;
                } else if (CanUpdateTransformItem()) {
                    CalculatePositionAndRotationItem(out Vector3 position, out Quaternion rotation);
                    currentItemInSocket.transform.SetPositionAndRotation(position, rotation);
                }
            }
        }

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            if (ghostSocket) {
                ghostSocketMeshFilter = ghostSocket.GetComponent<MeshFilter>();
                defaultMesh = ghostSocketMeshFilter.sharedMesh;
                defaultMeshTransform = ghostSocket.transform.Copy();
            }
            if (observer) {
                observer.RegisterEnter(AddPotentialItem, this);
                observer.RegisterExit(RemovePotentialItem, this);
            }
            if (!snapToTransform) {
                snapToTransform = transform;
            }
            maxInteractions = 1;
            OnChangeCanInteract();
        }

        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();
            if (observer != null) {
                observer.UnregisterEnter(this);
                observer.UnregisterExit(this);
            }
        }

        public bool IsSocketValid(ItemInteractor item) {
            if (item == null || item.InteractionItem == null) {
                return false;
            }
            if (!IsSocketFree(item.InteractionItem)) {
                return false;
            }
            //if (allowOnlyHeldItems && !item.InteractionItem.IsHeld) {
            //    return false;
            //}
            foreach (InteractionItem i in items) {
                if (i.Equals(item)) {
                    return true;
                }
            }
            foreach (SocketReference socketReference in item.InteractionItem.SocketReferences) {
                if (socketReferences.Contains(socketReference)) {
                    return true;
                }
            }
            return false;
        }

        public virtual void AddPotentialItem(ItemInteractor handler, GameObject source) {
            if (!potentialItems.Contains(handler)) {
                potentialItems.Add(handler);
                handler.RegisterPotentialInteraction(this);
                HandleGhost();
            }
        }

        public virtual void RemovePotentialItem(ItemInteractor handler, GameObject source) {
            if (potentialItems.Contains(handler)) {
                potentialItems.Remove(handler);
                handler.UnregisterPotentialInteraction(this);
                if (handler.Equals(currentGhostItem)) {
                    currentGhostItem = null;
                }
                HandleGhost();
            }
        }

        public bool CanAttractItem(ItemInteractor interactor) {
            InteractionItem item = interactor?.InteractionItem;
            return item != null && !item.IsHeld && IsSocketFree(item);
        }

        public void SetClosestItem(ItemInteractor item) {
            if (status == InteractionStatus.Active) {
                return;
            }
            PrepareGhost(item);
        }

        protected override void OnStartInteract(Interactor handler) {
            if (handler is ItemInteractor item) {
                wasFixedUpdate = false;
                InteractionItem interactionItem = item.InteractionItem;
                currentItemInSocket = item;
                interactionItem.Rigidbody.isKinematic = true;
                if (disableCollisionDuringHolding) {
                    interactionItem.SetCollisionColliders(false);
                }
                interactionItem.socket = this;
                interactionItem.transform.localScale = snapToTransform.localScale;
                onItemAttracted?.Invoke(interactionItem);
                onAction?.Invoke();
                HandleGhost();
            }
        }

        protected override void OnStopInteract(Interactor handler) {
            if (currentItemInSocket != null && handler == currentItemInSocket) {
                CurrentItemInSocket.Rigidbody.isKinematic = false;
                StopInteract(currentItemInSocket);
                if (disableCollisionDuringHolding) {
                    CurrentItemInSocket.SetCollisionColliders(true);
                }
                if (CurrentItemInSocket.socket == this) {
                    CurrentItemInSocket.socket = null;
                }
                CurrentItemInSocket.transform.localScale = Vector3.one;
                onItemDetracted?.Invoke(CurrentItemInSocket);
                currentItemInSocket = null;
                onAction?.Invoke();
                HandleGhost();
            }
        }

        protected override void OnChangeCanInteract() {
            if (status == InteractionStatus.Active) {
                CurrentItemInSocket.CanInteract = CanInteract;
            }
            HandleGhost();
        }

        protected void HandleGhost() {
            HandleGhost(CanShowGhost());
        }

        void HandleGhost(bool canShow) {
            if (ghostSocket) {
                ghostSocket.gameObject.SetActive(canShow);
            }
        }

        bool CanShowGhost() {
            if (ghostSocket) {
                if (currentItemInSocket != null || !CanInteract) {
                    return false;
                }
                bool showGhost = true;
                if (showGhostOnlyOnTrigger) {
                    showGhost &= potentialItems.Count > 0;
                }
                showGhost &= potentialItems.Any(i => CanShowGhostBasedOnItem(i));
                return showGhost;
            }
            return false;
        }

        bool CanShowGhostBasedOnItem(ItemInteractor item) {
            if (showGhostOnlyForLocalPlayer) {
                return item.IsOwner && IsSocketValid(item);
            } else {
                return IsSocketValid(item);
            }
        }

        void PrepareGhost(ItemInteractor item) {
            if (!ghostSocket || !item || item.Equals(currentGhostItem)) {
                return;
            }
            Transform ghostTransform = ghostSocketMeshFilter.transform;
            Mesh mesh = item.InteractionItem?.MeshFilter?.sharedMesh;
            if (mesh == null) {
                mesh = defaultMesh;
                ghostTransform.CopyValues(defaultMeshTransform);
            } else {
                Transform itemTransform = item.InteractionItem.transform;
                Transform meshTransform = item.InteractionItem.MeshFilter.transform;
                Vector3 offset = itemTransform.InverseTransformPoint(meshTransform.position);
                Vector3 position = snapToTransform.position + snapToTransform.TransformVector(offset);
                Quaternion rotationOffset = Quaternion.Inverse(itemTransform.rotation) * meshTransform.rotation;
                Quaternion rotation = snapToTransform.rotation * rotationOffset;
                ghostTransform.position = position;
                ghostTransform.rotation = rotation;
            }
            currentGhostItem = item;
            ghostSocketMeshFilter.mesh = mesh;
        }

        void CalculatePositionAndRotationItem(out Vector3 position, out Quaternion rotation) {
            position = snapToTransform.position;
            rotation = snapToTransform.rotation;
        }

        bool CanUpdateTransformItem() {
            return status == InteractionStatus.Active && currentItemInSocket != null;
        }

        bool IsSocketFree(InteractionItem item) {
            return item.socket == this || item.socket == null;
        }

    }
}

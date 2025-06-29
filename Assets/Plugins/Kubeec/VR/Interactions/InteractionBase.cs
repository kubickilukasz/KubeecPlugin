using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;
using System;
using System.Linq;
using Kubeec.VR.Character;
using Kubeec.VR.Outline;

namespace Kubeec.VR.Interactions {

    [DefaultExecutionOrder(1)]
    public abstract class InteractionBase : NetworkBehaviour {

        const NetworkVariableWritePermission writePermission = NetworkVariableWritePermission.Server;
        const NetworkVariableReadPermission readPermission = NetworkVariableReadPermission.Everyone;

        public UnityEvent onActiveEvent;
        public UnityEvent onInactiveEvent;
        public UnityEvent onStartInteract;
        public UnityEvent onStopInteract;

        public event Action onActive;
        public event Action onInactive;
        public event Action<bool> onChangeCanInteract;

        public InteractionBase parent { get; set; }       

        [SerializeField] protected int priority = 0;
        [SerializeField] protected int maxInteractions = 0;
        [SerializeField] protected CharacterHandInteraction interactionHand;

        public InteractionStatus status => _status.Value;
        [SerializeField] NetworkVariable<InteractionStatus> _status = new NetworkVariable<InteractionStatus>(InteractionStatus.Inactive, readPermission, writePermission);

        bool canInteract = true;
        public bool CanInteract {
            get {
                return canInteract;
            }
            set {
                if (canInteract != value) {
                    canInteract = value;
                    OnChangeCanInteract();
                    onChangeCanInteract?.Invoke(value);
                }
            }
        }

        public int Priority {
            set {
                priority = value;
            }
            get {
                return priority;
            }
        }
        public int CountHandlers => handlers.Count;
        public virtual bool Repeat => false;

        HashSet<Interactor> handlers = new HashSet<Interactor>();

        void Update() {
            if (Repeat) {
                CallOnStatus();
            }
            OnUpdate();
        }

        protected virtual void OnDisable() {
            StopAll();
        }

#if UNITY_EDITOR
        void OnDrawGizmos() {
            if (MyEditorSettings.TypeSetting.ShowInteractionGizmos.GetToggle()) {
                UnityEditor.Handles.color = Color.blue;
                UnityEditor.Handles.Label(transform.position, $"{name} {status} {CountHandlers}");
            }
        }
#endif

        public override void OnNetworkSpawn() {
            MyDebug.Log(MyDebug.TypeLog.Network, $"Interaction {name} spawned");
            SetMaxInteractions(maxInteractions);
            CallOnStatus();
            if (handlers.Count == 0) { 
                onStopInteract?.Invoke(); 
            } else { 
                onStartInteract?.Invoke(); 
            }
            _status.OnValueChanged += OnStatusChanged;
        }

        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();
            _status.OnValueChanged -= OnStatusChanged;
            StopAll();
        }

        public virtual bool CanInteractWithOthers() => true;

        public virtual CharacterHandInteraction GetCharacterHand(HandInteractor handler) => interactionHand;

        public virtual InteractionPositionData GetPositionData(Interactor handler) {
            return new InteractionPositionData(this, transform.position);
        }

        public bool StartInteract(Interactor handler) {
            if (CanInteract && handler != null && isActiveAndEnabled && maxInteractions > handlers.Count && handler.StartInteract(this)) {
                if (!handlers.Contains(handler)) {
                    handlers.Add(handler);
                    OnStartInteract(handler);
                    onStartInteract?.Invoke();
                }
                if (gameObject != null && handler.IsServer) {
                    CallOnStatus(InteractionStatus.Active);
                }
                return true;
            }
            return false;
        }

        public bool StopInteract(Interactor handler) {
            if (handler != null) {
                if (handlers.Contains(handler)) {
                    handlers.Remove(handler);
                    handler.StopInteract(this);
                    if (gameObject != null) {
                        OnStopInteract(handler);
                        onStopInteract?.Invoke();
                        if (handlers.Count == 0 && handler.IsServer ) {
                            CallOnStatus(InteractionStatus.Inactive);
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public IEnumerable<Interactor> GetHandlers() {
            return handlers;
        }

        public bool IsInInteraction(Interactor handler) {
            return handlers.Contains(handler);
        }

        public void SetMaxInteractions(int maxInteractions) {
            if (maxInteractions <= 0) maxInteractions = int.MaxValue;
            this.maxInteractions = maxInteractions;
        }

        protected virtual void OnInactive() {
        }

        protected virtual void OnActive() {
        }

        protected virtual void OnUpdate() {
        }

        protected virtual void OnStartInteract(Interactor handler) {
        }

        protected virtual void OnStopInteract(Interactor handler) {
        }

        protected virtual void OnChangeCanInteract() {
            if (!CanInteract) {
                StopAll();
            }
        }

        void OnInactiveInternal() {
            OnInactive();
            onInactive?.Invoke();
            onInactiveEvent?.Invoke();
        }

        void OnActiveInternal() {
            OnActive();
            onActive?.Invoke();
            onActiveEvent?.Invoke();
        }

        void CallOnStatus(InteractionStatus status) {
            if (IsSpawned) {
                CallOnStatusServerRpc(status);
            }
        }

        void CallOnStatus() {
            CallOnStatusLocal(status);
        }

        void CallOnStatusLocal(InteractionStatus status) {
            switch (status) {
                case InteractionStatus.Inactive: OnInactiveInternal(); break;
                case InteractionStatus.Active: OnActiveInternal(); break;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        void CallOnStatusServerRpc(InteractionStatus status) {
            _status.Value = status;
            return;
        }

        void StopAll() {
            while (handlers != null && handlers.Count > 0) {
                StopInteract(handlers.First());
            }
        }

        void OnStatusChanged(InteractionStatus oldStatus, InteractionStatus newStatus) {
            if (!Repeat) {
                CallOnStatus();
            }
        }

    }

    public enum InteractionStatus {
        Inactive = 0,
        Active = 1
    }

    [Serializable]
    public struct InteractionPositionData {
        public InteractionBase interaction { get; private set; }
        public HandHandler handHandler { get; private set; }
        public Vector3 position { get; private set; }
        public IOutlineable outlineable { get; private set; }

        public InteractionPositionData(InteractionBase interaction, Vector3 position) {
            handHandler = null;
            outlineable = null;
            this.position = position;
            this.interaction = interaction;
        }

        public InteractionPositionData(InteractionBase interaction, Vector3 position, IOutlineable outlineable, HandHandler handHandler) {
            this.interaction = interaction;
            this.outlineable = outlineable;
            this.handHandler = handHandler;
            this.position = position;
        }
    }

}

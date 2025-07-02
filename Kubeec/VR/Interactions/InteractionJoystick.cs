using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;

namespace Kubeec.VR.Interactions {

    public class InteractionJoystick : InteractionAttractedHand, IStateVector2, IAction {

        public event Action<Vector2> onChangeState;
        public event Action onAction;

        const NetworkVariableWritePermission writePermission = NetworkVariableWritePermission.Server;
        const NetworkVariableReadPermission readPermission = NetworkVariableReadPermission.Everyone;

        [SerializeField] protected NetworkVariable<Vector2> state = new NetworkVariable<Vector2>(Vector2.zero, readPermission, writePermission);

        [SerializeField] float angle = 30f;
        [SerializeField] Vector2 multiplierVisualization;
        [SerializeField] Vector2 multiplierInput;
        [SerializeField] bool flipAxisVisualization = false;
        [SerializeField] bool flipAxisInput = false;
        [SerializeField] Transform pivot;
        [SerializeField] Transform joystick;
        [Space]
        [SerializeField] bool forceState = false;
        [SerializeField] Vector2[] defaultStates;
        [SerializeField] float forceToDefaultState = 1f;
        [Space]
        [SerializeField] InputInteraction actionButton = InputInteraction.Select;
        [SerializeField] bool onHoldAction = false;

        Func<HandInteractor, bool> actionFunc;


        Vector2 tempStateValue;

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            ChangePosAndRotByState();
            state.OnValueChanged += OnStateChange;
            if (onHoldAction) {
                actionButton.GetInputs(out _, out actionFunc, out _);
            } else {
                actionButton.GetInputs(out actionFunc, out _, out _);
            }
        }

        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();
            state.OnValueChanged -= OnStateChange;
        }

        public Vector2 GetState() {
            Vector2 stateValue = IsServer ? state.Value : tempStateValue;
            return stateValue;
        }

        public void SetState(Vector2 value) {
            if (IsServer) {
                state.Value = value;
            }
        }

        public void SetDefaultStates(Vector2[] states) {
            defaultStates = states;
        }

        protected override void OnAttractedUpdate() {
            foreach (HandInteractor handler in GetHandlers()) {
                if (actionFunc.Invoke(handler)) {
                    onAction?.Invoke();
                    break;
                }
            }
        }

        protected override void OnStopInteract(Interactor handler) {
            (handler as HandInteractor).StopOverridePositionAndRotation(this);
        }

        [NonSerialized] HandHandlerData handHandlerData;
        protected override void OnActive() {
            if (IsServer) {
                Vector2 currentState = Vector2.zero;
                foreach (HandInteractor handler in GetHandlers()) {
                    HandHandler snapped = reservedPlacesToHandle[handler];
                    handHandlerData = snapped.Get(transform, handler);
                    handler.TryOverridePositionAndRotation(this, handHandlerData.handPosition, handHandlerData.handRotation);
                    currentState += GetForceFromHand(handler.controller);
                }
                ChangePosition(currentState);
            }
        }

        protected override void OnInactive() {
            if (IsServer) {
                if (forceState && defaultStates.Length > 0) {
                    float distance = float.MaxValue;
                    Vector2 currentState = Vector2.zero;
                    for (int i = 0; i < defaultStates.Length; i++) {
                        float cdistance = (defaultStates[i] - state.Value).magnitude;
                        if (cdistance < distance) {
                            distance = cdistance;
                            currentState = defaultStates[i];
                        }
                    }
                    if (!Mathf.Approximately(distance, 0f)) {
                        ChangePosition(forceToDefaultState * currentState * distance);
                    }
                }
            }
        }

        protected virtual void OnStateChange(Vector2 oldValue, Vector2 newValue) {
            ChangePosAndRotByState();
        }

        Vector2 GetForceFromHand(HandController hand) {
            Quaternion handRotation = hand.GetInputHandRotation();
            Quaternion diffQuaternion = Quaternion.Inverse(pivot.rotation) * handRotation;
            return new Vector2(
                (diffQuaternion * pivot.up).x,
                (diffQuaternion * pivot.up).z
                );
        }

        void ChangePosAndRotByState() {
            Vector2 stateValue = GetState();
            Vector2 pos = stateValue;
            if (flipAxisVisualization) {
                (pos.x, pos.y) = (pos.y, pos.x);
            }
            pos.x = Mathf.Clamp(pos.x * multiplierVisualization.x, -1f, 1f);
            pos.y = Mathf.Clamp(pos.y * multiplierVisualization.y, -1f, 1f);
            joystick.localRotation = Quaternion.Euler(pos.x * angle, 0f, pos.y * angle);
            onChangeState?.Invoke(stateValue);
        }

        void ChangePosition(Vector2 pos) {
            if (IsSpawned) {
                if (flipAxisInput) {
                    (pos.x, pos.y) = (pos.y, pos.x);
                }
                pos.x = Mathf.Clamp(pos.x * multiplierInput.x, -1f, 1f);
                pos.y = Mathf.Clamp(pos.y * multiplierInput.y, -1f, 1f);
                state.Value = pos;
            }
        }

    }

}

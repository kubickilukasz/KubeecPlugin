using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Networking;
using NaughtyAttributes;
using System;

namespace Kubeec.VR.Interactions {

    public class InteractionLever : InteractionAttractedHand, IStateFloat {

        public event Action<float> onChangeState;

        const float constForceMultiplier = 100f;

        const NetworkVariableWritePermission writePermission = NetworkVariableWritePermission.Server;
        const NetworkVariableReadPermission readPermission = NetworkVariableReadPermission.Everyone;

        [SerializeField] protected NetworkVariable<float> state = new NetworkVariable<float>(0, readPermission, writePermission);
        [SerializeField] protected NetworkVariable<float> force = new NetworkVariable<float>(0, readPermission, writePermission);

        [SerializeField] Transform startLever;
        [SerializeField] Transform endLever;
        [SerializeField] Transform lever;
        [SerializeField] float forceNeedToChange = 0.0f;
        [SerializeField] float forceMultiplier = 1f;
        [Space]
        [SerializeField] bool useSteps = false;
        [SerializeField] [ShowIf("useSteps")] int steps = 2;
        [Space]
        [InfoBox("To snap lever to some values when is not using by hand")]
        [SerializeField] bool forceState = false;
        [SerializeField] float[] defaultStates;
        [SerializeField] float forceToDefaultState = 1f;

        Vector3 positiveForceDirection => endLever.position - startLever.position;

        float tempStateValue = 0f;

        public override void OnNetworkSpawn() {
            base.OnNetworkSpawn();
            ChangePosAndRotByState();
            state.OnValueChanged += OnStateChange;
        }

        public override void OnNetworkDespawn() {
            base.OnNetworkDespawn();
            state.OnValueChanged -= OnStateChange;
        }

        public float GetState() {
            float stateValue = IsServer ? state.Value : tempStateValue;
            if (useSteps) {
                int stepDiv = Mathf.Max(1, steps - 1);
                stateValue *= stepDiv;
                int stateValueInt = Mathf.RoundToInt(stateValue);
                stateValue = (float)stateValueInt / stepDiv;
            }
            return stateValue;
        }

        public void SetState(float value) {
            if (IsServer) {
                state.Value = value;
            }
        }

        public void SetDefaultStates(float [] states) {
            defaultStates = states;
        }

        protected override void OnStopInteract(Interactor handler) {
            (handler as HandInteractor).StopOverridePositionAndRotation(this);
        }

        [NonSerialized] HandHandlerData handHandlerData;
        protected override void OnActive() {
            if (IsServer) {
                float currentForce = 0f;
                foreach (HandInteractor handler in GetHandlers()) {
                    HandHandler snapped = reservedPlacesToHandle[handler];
                    handHandlerData = snapped.Get(transform, handler);
                    handler.TryOverridePositionAndRotation(this, handHandlerData.handPosition, handHandlerData.handRotation);
                    currentForce += GetForceFromVector(handler.controller.GetForceVector());
                }
                if (Mathf.Abs(currentForce) > forceNeedToChange) {
                    PushLever(GetCalculatedForce(currentForce));
                }
            } else {
                tempStateValue = GetStateBasedOnForce(state.Value, force.Value);
            }
        }

        protected override void OnInactive() {
            if (IsServer) {
                if (!forceState || defaultStates.Length == 0) {
                    force.Value = 0f;
                } else {
                    float distance = float.MaxValue;
                    for (int i = 0; i < defaultStates.Length; i++) {
                        float cdistance = defaultStates[i] - state.Value;
                        if (Math.Abs(cdistance) < Math.Abs(distance)) {
                            distance = cdistance;
                        }
                    }
                    if (Mathf.Approximately(distance, 0f)) {
                        if (!Mathf.Approximately(force.Value, 0f)) {
                            PushLever(0f);
                        }
                    } else {
                        PushLever(forceToDefaultState * distance);
                    }
                }
            } else {
                tempStateValue = GetStateBasedOnForce(state.Value, force.Value);
            }
        }

        protected virtual void OnStateChange(float oldValue, float newValue) {
            ChangePosAndRotByState();
        }

        float GetForceFromVector(Vector3 forceVector) {
            Vector3 forceVectorNormalized = forceVector.normalized;
            float factorPos = Vector3.Dot(forceVectorNormalized, positiveForceDirection.normalized);
            Vector3 startDir = startLever.rotation * Vector3.up;
            Vector3 endDir = endLever.rotation * Vector3.up;
            Vector3 dir = endDir - startDir;
            float factorRot = Vector3.Dot(forceVectorNormalized, dir.normalized);

            return Mathf.Clamp(factorPos + factorRot,-1f,1f) * forceVector.magnitude * forceMultiplier;
        }

        void ChangePosAndRotByState() {
            float stateValue = GetState();
            lever.position = Vector3.Lerp(startLever.position, endLever.position, stateValue);
            lever.rotation = Quaternion.Lerp(startLever.rotation, endLever.rotation, stateValue);
            onChangeState?.Invoke(stateValue);
        }

        float GetCalculatedForce(float force) {
            return (force >= 0 ? force - forceNeedToChange : force + forceNeedToChange) * constForceMultiplier;
        }

        void PushLever(float force) {
            if (IsSpawned) {
                this.force.Value = force;
                if (force != 0) {
                    state.Value = GetStateBasedOnForce(state.Value, force);
                }
            }
        }

        float GetStateBasedOnForce(float currentState, float force) {
            return Mathf.Clamp01(currentState + (force * Time.deltaTime));
        }

    }
}

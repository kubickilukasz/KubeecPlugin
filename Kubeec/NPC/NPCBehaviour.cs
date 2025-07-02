using Kubeec.NPC.LazyNavigation;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kubeec.NPC {

    public abstract class NPCBehaviour : NPCInitable {

        NPCBehaviourState currentState;
        public NPCBehaviourState CurrentState => currentState;
        public ZoneController ZoneController { get; set; }

        void Update() {
            if (IsInitialized() && currentState != null) {
                currentState.Update();
            }
        }

        void FixedUpdate() {
            if (IsInitialized() && currentState != null) {
                currentState.FixedUpdate();
            }
        }

        public abstract NPCBehaviourState GetDefault();

        public T ChangeState<T>(T state) where T : NPCBehaviourState {
            if (!IsInitialized()) {
                return null;
            }
            if (currentState != null) {
                currentState.Dispose();
            }
            if (state != null) {
                currentState = state;
            }
            return state;
        }

        public bool IsState<T>() where T : NPCBehaviourState {
            return currentState is T;
        }

        protected override void OnInit(NonPlayerCharacter data) {
            base.OnInit(data);
            this.InvokeNextFrame(() => {
                if (IsInitialized()) {
                    currentState = GetDefault();
                }
            });
        }

        protected override void OnDispose() {
            base.OnDispose();
            if (currentState != null) {
                currentState.Dispose();
                currentState = null;
            }
        }

    }

    public abstract class NPCBehaviourState : IDisposable {

        protected NPCBehaviour behaviour;
        protected ZoneController zoneController;
        protected Zone currentZone;

        protected List<Vector3> path;

        public NPCBehaviourState(NPCBehaviour behaviour) {
            this.behaviour = behaviour;
            zoneController = behaviour.ZoneController;
            currentZone = zoneController.FindZone(position);
        }

        public Vector3 position => behaviour.transform.position;

        public virtual void Update() { }

        public virtual void FixedUpdate() { }

        public virtual void Dispose() {
        }

        protected Vector3? GetCurrentPositionToZone(Zone targetZone) {
            if (path == null) {
                path = zoneController.FindPath(currentZone, targetZone, behaviour.transform.position);
                if (path == null) {
                    return null;
                }
            }
            while (path.Count > 0) {
                Vector3 firstOnPath = path.First();
                if (behaviour.Character.Move.IsClose(firstOnPath)) {
                    currentZone = zoneController.FindZone(firstOnPath);
                    path.Remove(firstOnPath);
                } else {
                    return firstOnPath;
                }
            }
            path = null;
            return position;
        }

        
    }

}
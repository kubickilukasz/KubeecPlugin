using Kubeec.NPC.LazyNavigation;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kubeec.NPC {

    public abstract class NPCBehaviour : NPCInitable {

        [SerializeField] ZoneController zoneController;

        NPCBehaviourState currentState;
        public NPCBehaviourState CurrentState => currentState;
        public ZoneController ZoneController => zoneController;

        void Update() {
            if (IsInitialized()) {
                currentState.Update();
            }
        }

        void FixedUpdate() {
            if (IsInitialized()) {
                currentState.FixedUpdate();
            }
        }

        public abstract NPCBehaviourState GetDefault();

        public T ChangeState<T>(T state) where T : NPCBehaviourState {
            if (state != null) {
                currentState = state;
            }
            Debug.Log(typeof(T).Name);
            return state;
        }

        public bool IsState<T>() where T : NPCBehaviourState {
            return currentState is T;
        }

        protected override void OnInit(NonPlayerCharacter data) {
            base.OnInit(data);
            currentState = GetDefault();
        }

    }

    public abstract class NPCBehaviourState {

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

        protected Vector3? GetCurrentPositionToZone(Zone targetZone) {
            if (path == null) {
                path = zoneController.FindPath(currentZone, targetZone);
                if (path == null) {
                    return null;
                }
            }
            for (int i = 0; i < path.Count - 1; i++) {
                Debug.DrawLine(path[i], path[i + 1], Color.green);
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
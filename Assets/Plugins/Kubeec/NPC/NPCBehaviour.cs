using System;
using System.Collections;
using UnityEngine;

namespace Kubeec.NPC {

    public abstract class NPCBehaviour : NPCInitable {

        NPCBehaviourState currentState;
        public NPCBehaviourState CurrentState => currentState;

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
            return state;
        }

        protected override void OnInit(NonPlayerCharacter data) {
            base.OnInit(data);
            currentState = GetDefault();
        }

    }

    public abstract class NPCBehaviourState {

        protected NPCBehaviour behaviour;

        public NPCBehaviourState(NPCBehaviour behaviour) {
            this.behaviour = behaviour;
        }

        public virtual void Update() { }

        public virtual void FixedUpdate() { }


    }

}
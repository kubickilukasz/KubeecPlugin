using UnityEngine;

namespace Kubeec.NPC {

    public abstract class NPCMove : NPCInitable {

        const float minDistanceToClose = 0.25f;

        public abstract Vector3 GetDirection();
        public abstract bool IsOnGround();
        public virtual float GetCurrentSpeed() => GetDirection().magnitude;

        public void Move(Vector3 direction) {
            if (IsInitialized()) {
                OnMove(direction);
            }
        }

        public void MoveTo(Vector3 target) {
            if (IsInitialized()) {
                OnMoveTo(target);
            }
        }

        public void RotateLook(Vector3 position) {
            if (IsInitialized()) {
                OnRotateLook(position);
            }
        }

        public void Stop() {
            if (IsInitialized()) {
                OnStop();
            }
        }

        public virtual bool IsClose(Vector3 position) {
            return (position - transform.position).sqrMagnitude < (minDistanceToClose * minDistanceToClose);
        }

        protected abstract void OnMove(Vector3 direction);

        protected abstract void OnMoveTo(Vector3 direction);

        protected abstract void OnRotateLook(Vector3 position);

        protected abstract void OnStop();

    }

}

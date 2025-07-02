using Kubeec.VR.Character;
using UnityEngine;

namespace Kubeec.VR.Player {

    [DefaultExecutionOrder(-6)]
    public class PlayerMove : EnableDisableInitableDisposable<PlayerMoveData> {

        [SerializeField] Rigidbody rigidbody;
        [SerializeField] float speedMovement = 2f;
        [SerializeField] float accMovement = 2f;
        [SerializeField] float gravity = 9f;
        [SerializeField] float maxDistanceFromHeadToControllerToReset = 2.5f;
        [SerializeField] float speedInputToHead = 3f;
        [SerializeField] float speedToInput = 5f;

        public Vector3 MoveValue => move;
        public Vector3 Velocity => rigidbody.linearVelocity;

        PlayerControllerBase playerInput;
        CharacterSetup setup;

        Vector3 directionMove;
        Vector3 move;
        Vector3 prevPos;
        float yPosition = 0f;
        bool canMoveInput = true;

        void Update() {
            if (IsInitialized()) {
                ReadInput();
            }  
        }

        private void LateUpdate() {
            if (IsInitialized()) {
                MoveInput(Time.deltaTime);
            }
        }

        void FixedUpdate() {
            if (IsInitialized()) {
                Move(Time.fixedDeltaTime);
            }
        }

        public void ResetPosition(Vector3? newPos = null, Quaternion? newRot = null) {
            Debug.Log($"ResetPosition {newPos} {newRot}");
            if (!newPos.HasValue) {
                newPos = Vector3.zero;
            }
            move = directionMove = Vector3.zero;
            rigidbody.position = newPos.Value;
            if (newRot.HasValue) {
                rigidbody.rotation = newRot.Value;
            }
            if (playerInput) {
                //playerInput.SetPosition(newPos.Value);
                playerInput.transform.localPosition = newPos.Value;
                if (newRot.HasValue) {
                    playerInput.transform.rotation = newRot.Value;
                }
            }
            rigidbody.linearVelocity = Vector3.zero;
            canMoveInput = true;
        }

        protected override void OnInit(PlayerMoveData data) {
            if (data == null) {
                Dispose();
                return;
            }
            playerInput = data.controllerBase;
            setup = data.characterSetup;
            rigidbody.isKinematic = false;
            rigidbody.detectCollisions = true;
        }

        protected override void OnDispose() {
            move = Vector3.zero;
            rigidbody.isKinematic = true;
            rigidbody.detectCollisions = false;
        }

        void ReadInput() {
            Vector2 dir2D = playerInput.GetJoyLeftHand();
            Vector3 localDir3D = new Vector3(dir2D.x, 0, dir2D.y);
            directionMove = Quaternion.Euler(0, playerInput.GetHeadRotation().eulerAngles.y, 0) * localDir3D;
        }

        void Move(float deltaTime) {
            Vector3 bodyOffset = GetDistanceToInput();
            float distance = bodyOffset.magnitude;
            if (distance > maxDistanceFromHeadToControllerToReset) {
                ResetPosition(transform.position);
                return;
            }

            move = directionMove * speedMovement;
            move -= bodyOffset * speedToInput;
            rigidbody.AddForce((move - rigidbody.linearVelocity) * (deltaTime * accMovement), ForceMode.VelocityChange);
        }

        void MoveInput(float deltaTime) {
            Vector3 dirToInput = GetDistanceToInput();
            Vector3 dirToHead = transform.position - GetHeadPosition();
            dirToHead.y = 0f;
            float sqrDirToHead = dirToHead.sqrMagnitude;
            if (sqrDirToHead > 0f) {
                float w = dirToInput.sqrMagnitude / sqrDirToHead;
                Vector3 dir = (dirToInput * w) + (dirToHead * speedInputToHead / w);
                Vector3 target = playerInput.GetBodyPosition() + (dir * deltaTime);
                playerInput.SetPosition(target);
            }
        }

        Vector3 GetDistanceToInput() {
            Vector3 bodyOffset = transform.position - GetHeadPosition();
            bodyOffset.y = 0f;
            return bodyOffset;
        }

        Vector3 GetHeadPosition() {
            return setup.headPosition;
        }

    }

    public class PlayerMoveData {
        public PlayerControllerBase controllerBase;
        public CharacterSetup characterSetup;
    }

}

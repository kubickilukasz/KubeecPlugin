using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Unity.Netcode;
using System;
using Kubeec.VR.Interactions;
using Kubeec.VR.Character;

namespace Kubeec.VR.Player {
    [DefaultExecutionOrder(-4)] //After PlayerControllerBase (f.e. OVR) before Item and Others
    public class PlayerController : NetworkBehaviour {

        public static Action onCameraUpdateBeforeRender = null;
        public event Action<CharacterSetup> onChangeCharacter;

        [SerializeField] CharacterTracker characterTracker;
        [SerializeField] CapsuleCollider capsuleCollider;
        [SerializeField] HandController leftHandController;
        [SerializeField] HandController rightHandController;
        [SerializeField] PlayerMove playerMove;
        [SerializeField] Vignette vignette;
        [SerializeField] PlayerRoom playerRoom;
        [Space]
        [SerializeField] float sqrDistanceMinToShowVignette = 1f;

        const NetworkVariableWritePermission writePermission = NetworkVariableWritePermission.Owner;
        const NetworkVariableReadPermission readPermission = NetworkVariableReadPermission.Everyone;

        NetworkVariable<float> gripValueLeftHand = new NetworkVariable<float>(1f, readPermission, writePermission);
        NetworkVariable<float> gripValueRightHand = new NetworkVariable<float>(1f, readPermission, writePermission);
        NetworkVariable<float> selectValueLeftHand = new NetworkVariable<float>(1f, readPermission, writePermission);
        NetworkVariable<float> selectValueRightHand = new NetworkVariable<float>(1f, readPermission, writePermission);

        public float GripValueLeftHand => gripValueLeftHand.Value;
        public float GripValueRightHand => gripValueRightHand.Value;
        public float SelectValueLeftHand => selectValueLeftHand.Value;
        public float SelectValueRightHand => selectValueRightHand.Value;
        public Vector3 Velocity => playerMove.Velocity;
        public Vignette Vignette => vignette;

        PlayerControllerBase playerInput;
        LocalPlayerReference localPlayerReference;

        bool wasPressedLeftSelect, wasPressedRightSelect, wasPressedLeftGrip, wasPressedRightGrip;
        bool isPressedLeftSelect, isPressedRightSelect, isPressedLeftGrip, isPressedRightGrip;
        bool canInnerControl = true;
        bool wasPhyscisFrame = false;

        void Start() {
            if ((IsServer || IsHost) && !IsSpawned) {
                NetworkObject.Spawn();
            }
            playerInput.onShouldUpdateHead += UpdateBeforeRender;
        }

        public override void OnDestroy() {
            base.OnDestroy();
            if (playerInput) {
                playerInput.onShouldUpdateHead -= UpdateBeforeRender;
            }
        }

        void Update() {
            RefreshHandsValues();
            UpdateVignette();
            if (!canInnerControl) {
                return;
            }

            if (playerInput != null) {
                UpdateHeadAndHands();
                SetStateLeftHand(playerInput.GetPressValueLeftGrip(), playerInput.GetPressValueLeftSelect());
                SetStateRightHand(playerInput.GetPressValueRightGrip(), playerInput.GetPressValueRightSelect());
            } else {
                leftHandController.SetBruteForcePositionAndRotation(leftHandController.transform.position, leftHandController.transform.rotation);
                rightHandController.SetBruteForcePositionAndRotation(rightHandController.transform.position, rightHandController.transform.rotation);
            }

            leftHandController.SetGripAndSelect(gripValueLeftHand.Value, selectValueLeftHand.Value,
                IsPressedDownLeftGrip(), IsPressedDownLeftSelect(),
                IsPressedUpLeftGrip(), IsPressedUpLeftSelect());
            rightHandController.SetGripAndSelect(gripValueRightHand.Value, selectValueRightHand.Value,
                IsPressedDownRightGrip(), IsPressedDownRightSelect(),
                IsPressedUpRightGrip(), IsPressedUpRightSelect());

            UpdateHandsTransform();

            leftHandController.UpdateFingers();
            rightHandController.UpdateFingers();
        }

        void FixedUpdate() {
            if (playerInput != null) {
                UpdateCharacterCollider();
                UpdateHeadAndHands();
                UpdateHandsPhysics();
                wasPhyscisFrame = true; 
            }
        }

        void LateUpdate() {
            UpdateFingersAndShoulders();
            wasPhyscisFrame = false;
        }

        public override void OnNetworkSpawn() {
            Camera camera = null;
            if (IsOwner) {
                playerInput = PlayerControllerBase.instance;
                playerInput.transform.SetParent(transform, false);
                localPlayerReference = LocalPlayerReference.instance;
                camera = localPlayerReference.Camera;
                vignette.Init(new VignetteData() { camera = camera.transform });
                playerMove.Init(new PlayerMoveData() { 
                    controllerBase = playerInput,
                    characterSetup = characterTracker.CharacterSetup
                });
                ChangeCharacter();
            } else {
                playerMove.Dispose();
                Destroy(playerRoom.gameObject);
                Destroy(vignette.gameObject);
            }
            canInnerControl = true;
            characterTracker.Init(this, null);
            leftHandController.Init(characterTracker.CharacterSetup.LeftHandSetup, camera, false, IsOwner);
            rightHandController.Init(characterTracker.CharacterSetup.RightHandSetup, camera, true, IsOwner);
        }

        public void ChangeCharacter() {
            CharacterSetup setup = characterTracker.CharacterSetup;
            Collider[] colliders = setup.RagdollSetup.Colliders;
            leftHandController.IgnoreCollision(colliders);
            rightHandController.IgnoreCollision(colliders);
            onChangeCharacter?.Invoke(setup);
        }

        public void ResetPosition(Vector3? newPos = null, Quaternion? newRot = null) {
            playerMove.ResetPosition(newPos, newRot);
        }

        public void SetPlayerRoom(bool inRoom) {
            if (playerRoom != null) {
                if (inRoom) {
                    playerRoom.Show(characterTracker.HeadTracker.transform.rotation.eulerAngles.y);
                } else {
                    playerRoom.Hide();
                }
            }
        }

        public void SetPlayerSpace(PlayerSpace space) {
            Debug.Log($"Player Space {space}");
            if (localPlayerReference == null) {
                return;
            }
            localPlayerReference.Camera.cullingMask = space.CameraLayerMask;
            gameObject.layer = LayerMask.NameToLayer(space.DefaultLayer);
            canInnerControl = space.CanInteract && !space.Ragdoll;
            characterTracker.CharacterSetup.gameObject.SetActive(!space.HidePlayerObject);
            foreach (HandController handController in GetHands()) {
                handController.ResetHand(space.RaycastItem);
                handController.gameObject.layer = LayerMask.NameToLayer(space.HandLayer);
            }
            SetPlayerRoom(space.ShowPlayerRoom);
            characterTracker.SetRagdoll(space.Ragdoll);
            if (space.CanMove) {
                playerMove.Init(new PlayerMoveData() {
                    controllerBase = playerInput,
                    characterSetup = characterTracker.CharacterSetup
                });
            } else {
                playerMove.Dispose();
            }
        }

        public IEnumerable<HandController> GetHands() {
            yield return leftHandController;
            yield return rightHandController;
        }

        void UpdateCharacterCollider() {
            float height = playerInput.GetHeadPosition().y - transform.position.y;
            float prevHeight = capsuleCollider.height;
            float currentCeilMargin = capsuleCollider.center.y - (prevHeight / 2f) - transform.position.y;
            height -= currentCeilMargin;
            capsuleCollider.height = height;
            capsuleCollider.center = new Vector3(
                capsuleCollider.center.x,
                capsuleCollider.center.y + (height - prevHeight) / 2f,
                capsuleCollider.center.z);
        }

        void UpdateVignette() {
            if (vignette != null) {
                if (playerMove.MoveValue.sqrMagnitude * 100 > sqrDistanceMinToShowVignette) {
                    vignette.Set(0.5f, 1);
                } else {
                    vignette.Set(0f, 0f);
                }
            }
        }

        void UpdateBeforeRender() {
            if (playerInput) {
                characterTracker.ForceUpdate();
                onCameraUpdateBeforeRender?.Invoke();
            }
        }

        void UpdateHeadAndHands() {
            characterTracker.SetHeadRotation(playerInput.GetHeadRotation());
            characterTracker.SetHeadPosition(playerInput.GetHeadPosition());
            leftHandController.SetPositionAndRotation(playerInput.GetPositionLeftHand(),
            playerInput.GetRotationLeftHand());
            rightHandController.SetPositionAndRotation(playerInput.GetPositionRightHand(),
                playerInput.GetRotationRightHand());
        }

        void UpdateFingersAndShoulders() {
            if (canInnerControl) {
                characterTracker.UpdateShoulders();
            }
        }

        void UpdateHandsPhysics() {
            leftHandController.UpdateHandPhysics();
            rightHandController.UpdateHandPhysics();
        }

        void UpdateHandsTransform() {
            leftHandController.UpdateHandTransform(wasPhyscisFrame, Velocity * Time.deltaTime);
            rightHandController.UpdateHandTransform(wasPhyscisFrame, Velocity * Time.deltaTime);
        }

        void SetStateLeftHand(float gripValue, float selectValue) {
            gripValueLeftHand.Value = gripValue;
            selectValueLeftHand.Value = selectValue;
        }

        void SetStateRightHand(float gripValue, float selectValue) {
            gripValueRightHand.Value = gripValue;
            selectValueRightHand.Value = selectValue;
        }

        void RefreshHandsValues() {
            wasPressedLeftSelect = isPressedLeftSelect;
            wasPressedRightSelect = isPressedRightSelect;
            wasPressedLeftGrip = isPressedLeftGrip;
            wasPressedRightGrip = isPressedRightGrip;
            if (playerInput != null) {
                isPressedLeftSelect = playerInput.IsPressedValueLeftSelect();
                isPressedRightSelect = playerInput.IsPressedValueRightSelect();
                isPressedLeftGrip = playerInput.IsPressedValueLeftGrip();
                isPressedRightGrip = playerInput.IsPressedValueRightGrip();
            } else {
                isPressedLeftSelect = PlayerControllerBase.IsPressed(selectValueLeftHand.Value);
                isPressedRightSelect = PlayerControllerBase.IsPressed(selectValueRightHand.Value);
                isPressedLeftGrip = PlayerControllerBase.IsPressed(gripValueLeftHand.Value);
                isPressedRightGrip = PlayerControllerBase.IsPressed(gripValueRightHand.Value);
            }
        }

        bool IsPressedDownLeftSelect() => !wasPressedLeftSelect && isPressedLeftSelect;
        bool IsPressedDownRightSelect() => !wasPressedRightSelect && isPressedRightSelect;
        bool IsPressedDownLeftGrip() => !wasPressedLeftGrip && isPressedLeftGrip;
        bool IsPressedDownRightGrip() => !wasPressedRightGrip && isPressedRightGrip;
        bool IsPressedUpLeftSelect() => wasPressedLeftSelect && !isPressedLeftSelect;
        bool IsPressedUpRightSelect() => wasPressedRightSelect && !isPressedRightSelect;
        bool IsPressedUpLeftGrip() => wasPressedLeftGrip && !isPressedLeftGrip;
        bool IsPressedUpRightGrip() => wasPressedRightGrip && !isPressedRightGrip;

    }
}

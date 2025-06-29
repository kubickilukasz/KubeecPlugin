using Kubeec.Hittable;
using Kubeec.VR.Character;
using Kubeec.VR.Player;
using Unity.Netcode;
using UnityEngine;

namespace Kubeec.VR {

    public class DeathPlayerHandler : EnableDisableInitableDisposable {

        [SerializeField] HitCollector hitCollector;
        [SerializeField] PlayerController playerController;
        [SerializeField] CharacterTracker characterTracker;
        [SerializeField] Vignette vignette;
        [SerializeField] Camera headCamera;

        Transform defaultCameraTransform;

        void Update() {
            if (IsInitialized() && SceneDeathPlayerHandler.Instance == null) {
                SetHeadCameraToHead();
            }
        }

        protected override void OnInit(object data) {
            hitCollector.onInit += HandleReborn;
            hitCollector.onDispose += HandleDeath;
            if (!defaultCameraTransform) {
                defaultCameraTransform = new GameObject("Default Head Camera Transform").transform;
                defaultCameraTransform.parent = headCamera.transform.parent;
                defaultCameraTransform.localPosition = headCamera.transform.localPosition;
                defaultCameraTransform.localRotation = headCamera.transform.localRotation;
            }
            
        }

        protected override void OnDispose() {
            hitCollector.onInit -= HandleReborn;
            hitCollector.onDispose -= HandleDeath;
        }

        public void Reborn(Vector3? rebornPosition = null) {
            hitCollector.Init();
            playerController.ResetPosition(rebornPosition);
        }

        public void SetHeadCamera(Vector3 position, Quaternion rotation) {
            headCamera.transform.position = position;
            headCamera.transform.rotation = rotation;
        }

        public void SetHeadCameraToHead() {
            headCamera.transform.position = characterTracker.CharacterSetup.headPosition;
            headCamera.transform.rotation = characterTracker.CharacterSetup.headRotation;
        }

        void HandleReborn() {
            vignette.SetFadeInFadeOut(() => true, () => {
                headCamera.transform.localPosition = defaultCameraTransform.localPosition;
                headCamera.transform.localRotation = defaultCameraTransform.localRotation;
                playerController.SetPlayerRoom(false);
                playerController.SetRagdoll(false);
            }, null);
        }

        void HandleDeath() {
            vignette.SetFadeInFadeOut(() => true, () => {
                playerController.SetPlayerRoom(true);
                playerController.SetRagdoll(true);
            }, () => {
                if (playerController.IsServer) {
                    if (SceneDeathPlayerHandler.Instance != null) {
                        SceneDeathPlayerHandler.Instance.HandleDeath(this);
                    } else {
                        this.InvokeDelay(() => {
                            Reborn();
                        }, 10f);
                    }
                }
            });
        }

    }

}

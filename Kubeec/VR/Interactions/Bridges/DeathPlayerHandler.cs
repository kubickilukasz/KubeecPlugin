using Kubeec.Hittable;
using Kubeec.VR.Character;
using Kubeec.VR.Player;
using Unity.Netcode;
using UnityEngine;

namespace Kubeec.VR {

    public class DeathPlayerHandler : EnableDisableInitableDisposable {

        [SerializeField] HitCollector hitCollector;
        [SerializeField] PlayerSpace defaultDeathSpace;
        [SerializeField] PlayerSpace defaultRebornSpace;
        [SerializeField] PlayerController playerController;
        [SerializeField] CharacterTracker characterTracker;
        [SerializeField] Vignette vignette;
        [SerializeField] Camera headCamera;

        Transform defaultCameraTransform;
        Vector3? rebornPosition;
        Quaternion? rebornRotation;

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

        public void Reborn(Vector3? rebornPosition = null, Quaternion? rebornRotation = null) {
            Debug.Log("Reborn");
            hitCollector.Init();
            this.rebornPosition = rebornPosition;
            this.rebornRotation = rebornRotation;
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
                playerController.ResetPosition(rebornPosition, rebornRotation);
                playerController.SetPlayerSpace(defaultRebornSpace);
            }, null);
        }

        void HandleDeath() {
            SceneDeathPlayerHandler sceneDeathPlayerHandler = SceneDeathPlayerHandler.Instance;
            vignette.SetFadeInFadeOut(() => true, () => {
                PlayerSpace space = null;
                if (sceneDeathPlayerHandler != null) {
                    space = sceneDeathPlayerHandler.GetPlayerSpace();
                }
                space = space == null ? defaultDeathSpace : space;
                playerController.SetPlayerSpace(space);
            }, () => {
                if (playerController.IsServer) {
                    if (sceneDeathPlayerHandler != null) {
                        sceneDeathPlayerHandler.HandleDeath(this);
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

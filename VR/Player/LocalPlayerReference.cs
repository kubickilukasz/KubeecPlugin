using Kubeec.VR.Outline;
using System;
using System.Collections;
using UnityEngine;

namespace Kubeec.VR.Player {
    public class LocalPlayerReference : Singleton<LocalPlayerReference> {

        static event Action<PlayerController> onPlayer;

        [SerializeField] Camera camera;
        public Camera Camera => camera;

        [SerializeField] OutlineController outlineController;
        public OutlineController OutlineController => outlineController;

        PlayerController playerController;
        public PlayerController PlayerController => playerController;

        protected override void OnInit() {
            StartCoroutine(WaitForController());
        }

        public static void SafeGetPlayerController(Action<PlayerController> onGet) {
            if (!instanceExist || instance.PlayerController == null) {
                onPlayer += onGet;
            } else {
                onGet?.Invoke(instance.PlayerController);
            }
        }

        IEnumerator WaitForController() {
            do {
                yield return null;
                playerController = GetComponentInParent<PlayerController>();
            } while (playerController == null);
            onPlayer?.Invoke(playerController);
            onPlayer = null;
        }

    }
}

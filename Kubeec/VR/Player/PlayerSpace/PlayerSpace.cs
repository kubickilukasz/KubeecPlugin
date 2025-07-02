using Kubeec.VR.Interactions;
using UnityEngine;

namespace Kubeec.VR.Player {

    [CreateAssetMenu(fileName = "PlayerSpace", menuName = "ScriptableObjects/PlayerSpace")]
    public class PlayerSpace : ScriptableObject, IPlayerSpace {

        [SerializeField] string defaultLayer;
        [SerializeField] string handLayer;
        [SerializeField] LayerMask cameraLayerMask;
        [SerializeField] bool canMove;
        [SerializeField] bool canInteract;
        [SerializeField] bool ragdoll;
        [SerializeField] bool hidePlayerObject;
        [SerializeField] bool raycastItem;
        [SerializeField] bool showPlayerRoom;

        public string DefaultLayer => defaultLayer;
        public string HandLayer => handLayer;
        public bool CanMove => canMove;
        public LayerMask CameraLayerMask => cameraLayerMask;
        public bool CanInteract => canInteract;
        public bool Ragdoll => ragdoll;
        public bool HidePlayerObject => hidePlayerObject;
        public bool RaycastItem => raycastItem;
        public bool ShowPlayerRoom => showPlayerRoom;

    }

    public interface IPlayerSpace {
        public string DefaultLayer { get;}
        public string HandLayer { get; }
        public LayerMask CameraLayerMask { get; }
        public bool CanMove { get; }
        public bool CanInteract { get; }
        public bool Ragdoll { get; }
        public bool HidePlayerObject { get; }
        public bool RaycastItem { get; }
        public bool ShowPlayerRoom { get; }
    }

}

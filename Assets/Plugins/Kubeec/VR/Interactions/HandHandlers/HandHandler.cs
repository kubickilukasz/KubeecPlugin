using Kubeec.VR.Character;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;
using NaughtyAttributes;
using Kubeec.VR.Outline;
using System.Collections.Generic;
using Kubeec.General;

namespace Kubeec.VR.Interactions {

    [ExecuteAlways]
    public abstract class HandHandler : InitableDisposable<HandHandlerBaseInitData>, IOutlineable{

        const float minHandDirectionZ = -0.2f;
        const float maxHandDirectionZ = 1.99f;

        [SerializeField][Expandable] protected CharacterHandInteraction handPose;
        [SerializeField] protected HandType handType = HandType.Both;
        [SerializeField] HoldType holdTypeForPosition;
        [SerializeField] HoldType holdTypeForRotation;
        [SerializeField, Range(-1, 1)] protected float handDirectionX = 0f;
        [SerializeField, Range(-1, 1)] protected float handDirectionY = 0f;
        [SerializeField, Range(minHandDirectionZ, maxHandDirectionZ)] protected float handDirectionZ = 1f;
        [SerializeField] protected Vector3 localOffsetPositionHand;
        [SerializeField] [Range(0,180f)] protected float maxAngle = 0;
        [SerializeField] [Range(0,1f)] protected float weight = 1f;
        [SerializeField] OutlineObject outlineObject;
       
        [HideInInspector][SerializeField] protected Vector3 localOffsetPositionHand_R;
        [HideInInspector][SerializeField] protected Quaternion localOffsetRotationHand_R;
        [HideInInspector][SerializeField] protected Quaternion handDirectionXRotation;

        protected OutlineObject[] outlineOutput = new OutlineObject[] { };
        protected Vector3 holdDirection = Vector3.right;

        void OnEnable() {
            Init();
#if UNITY_EDITOR
            UnityEditor.SceneView.duringSceneGui += OnSceneGUI;
#endif
        }

        public static T CreateDynamicHandHandler<T>(HandInteractor handler, Transform parent) where T : HandHandler{
            Vector3 worldPosition = handler.controller.GetInputHandPosition();
            T current = HandHandlersPool<T>.pool.Get();
            current.Dispose();
            Transform tr = current.transform;
            tr.SetParent(parent);
            tr.position = worldPosition;
            tr.localRotation = Quaternion.Inverse(parent.rotation) * handler.controller.GetInputHandRotation();
            HandHandlerBaseInitData data = new HandHandlerBaseInitData();
            data.holdDirection = (parent.position - worldPosition).normalized;
            data.holdDirection.x = Math.Abs(data.holdDirection.x);
            data.holdDirection.z = 0;
            data.weight = 1f;
            current.Init(data);
            return current;
        }

        public static void RemoveHandHandler<T>(T handHandler) where T : HandHandler {
            handHandler.Dispose();
            HandHandlersPool<T>.pool.Release(handHandler);
        }

        public CharacterHandInteraction GetHandPose() => handPose;

        public abstract HandHandlerData Get(Transform item, HandInteractor hand);

        public virtual Vector3 GetHandPosition(bool isRightHand) {
            return transform.TransformPoint(GetLocalPosition(isRightHand));
        }

        public bool CanUse(HandInteractor handler) {
            bool canUse = isActiveAndEnabled && gameObject.activeInHierarchy;
            if (maxAngle > 0) {
                canUse &= Angle(handler.controller.GetInputHandRotation()) <= maxAngle;
            }
            if (handType != HandType.Both) {
                canUse &= (handType == HandType.RightHand && handler.controller.IsRightHand) || (handType == HandType.LeftHand && handler.controller.IsLeftHand);
            }
            return canUse;
        }

        public bool CanOutline() {
            return outlineObject != null;
        }

        public IEnumerable<OutlineObject> GetOutlineObjects(Vector3 source) {
            return outlineOutput;
        }

        protected Quaternion GetItemRotation(HandInteractor hand) {
            return hand.controller.GetInputHandRotation() * GetItemRotation(hand.controller.IsRightHand);
        }

        protected Vector3 GetItemPosition(Transform item, Vector3 targetPosition) {
            return targetPosition + GetItemPositionOffset(item);
        }

        protected Vector3 GetItemPositionOffset(Transform item) {
            return item.position - item.TransformPoint(transform.localPosition);
        }

        protected Quaternion GetHandRotation(HandInteractor hand) {
            return GetHandRotation(hand.controller.GetInputHandRotation(), hand.controller.IsRightHand);
        }

        protected float Angle(Quaternion realHandRotation) {
            return Quaternion.Angle(transform.rotation, realHandRotation);
        }

        protected Quaternion GetHandRotation(Quaternion inputRotation, bool isRight) {
            return inputRotation * Quaternion.AngleAxis(Mathf.Lerp(-30, 30, (handDirectionX + 1) / 2f), Vector3.right) 
                * Quaternion.AngleAxis((isRight ? 1 : -1) * Mathf.Lerp(-45, 45, (handDirectionY + 1) / 2f), Vector3.up);
        }

        protected override void OnInit(HandHandlerBaseInitData data) {
            outlineOutput = new[] { outlineObject };
            if (data != null) {
                holdDirection = data.holdDirection;
                holdTypeForPosition = data.holdTypeForPosition;
                holdTypeForRotation = data.holdTypeForRotation;
                weight = data.weight;
            } else {
                handDirectionZ = Mathf.Clamp(handDirectionZ, minHandDirectionZ, maxHandDirectionZ);
                float temp = -Mathf.LerpUnclamped(0, 90f, 1f - handDirectionZ);
                holdDirection = Quaternion.AngleAxis(temp, Vector3.forward) * Vector3.right;
            }
            localOffsetPositionHand_R = SwitchPosition(localOffsetPositionHand);
            localOffsetRotationHand_R = SwitchRotation(transform.localRotation);
        }

        protected Vector3 GetLocalPosition(bool isRightHand) {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                return isRightHand ? SwitchPosition(localOffsetPositionHand) : localOffsetPositionHand;
            }
#endif
            return isRightHand ? localOffsetPositionHand_R : localOffsetPositionHand;
        }

        protected Vector3 SwitchPosition(Vector3 pos) {
            switch (holdTypeForPosition) {
                case HoldType.MirrorX: pos.x = -pos.x; break;
                case HoldType.MirrorY: pos.y = -pos.y; break;
                case HoldType.MirrorZ: pos.z = -pos.z; break;
            }
            return pos;
        }

        protected Quaternion SwitchRotation(Quaternion rot) {
            switch (holdTypeForRotation) {
                case HoldType.MirrorX: rot *= Quaternion.Euler(180f, 0, 0); break;
                case HoldType.MirrorY: rot *= Quaternion.Euler(0, 180f, 0); break;
                case HoldType.MirrorZ: rot *= Quaternion.Euler(0, 0, 180f); break;
            }
            return rot;
        }

        protected Quaternion GetItemRotation(bool isRightHand) {
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                return Quaternion.Inverse(isRightHand ? SwitchRotation(transform.localRotation) : transform.localRotation);
            }
#endif
            return Quaternion.Inverse(isRightHand ? localOffsetRotationHand_R : transform.localRotation);
        }

        public enum HoldType {
            None, MirrorX, MirrorY, MirrorZ
        }

        public enum HandType {
            Both, RightHand, LeftHand
        }

#region Visualization
#if UNITY_EDITOR
        void OnValidate() {
            OnInit(null);
            UnityEditor.SceneView.RepaintAll();
        }

        void OnDisable() {
            UnityEditor.SceneView.duringSceneGui -= OnSceneGUI;
        }

        protected virtual Material GetDebugMaterial() {
            return Resources.Load("GhostHandMaterial") as Material;
        }

        static Mesh _mesh;
        Mesh mesh {
            get {
                if (_mesh == null) {
                    GameObject obj = Resources.Load("Hand_LowPoly") as GameObject;
                    _mesh = obj.GetComponentInChildren<MeshFilter>().sharedMesh;
                }
                return _mesh;
            }
        }

        static Material _material;
        protected virtual Material material {
            get {
                if (_material == null) {
                    _material = GetDebugMaterial();
                }
                return _material;
            }
        }

        bool render;
        static Vector3 ghostHandOffsetPosition = new Vector3(-0.01f, 0.09f, -0.02f);
        static Quaternion ghostHandOffsetRotation = Quaternion.Euler(0, 270, 90);
        Transform parentInteraction;

        void OnSceneGUI(UnityEditor.SceneView sceneView) {
            if (render) {
                render = false;
                if (parentInteraction == null) {
                    parentInteraction = transform.parent;// GetComponentInParent<InteractionBase>(true);
                }

                if (handType != HandType.RightHand) {
                    DrawHand(false);
                }
                if (MyEditorSettings.TypeSetting.ShowBothGhostHandsInteractions.GetToggle() || handType == HandType.RightHand) {
                    DrawHand(true);
                }
            }

            void DrawHand(bool right) {
                Quaternion handDirection = Quaternion.Euler(0, 0f, 90f * (right ? (1f - handDirectionZ) : -(1f - handDirectionZ)));
                Quaternion itemRotation = parentInteraction ? parentInteraction.rotation * Quaternion.Inverse(GetItemRotation(right)) : GetItemRotation(right);
                Vector3 localPosition = GetLocalPosition(right);

                Matrix4x4 matrixHandModel = Matrix4x4.Rotate(ghostHandOffsetRotation) * Matrix4x4.Scale(new Vector3(1, 1, right ? -1 : 1)) * Matrix4x4.Translate(ghostHandOffsetPosition);
                Matrix4x4 matrixHandDirection = Matrix4x4.Rotate(handDirection);
                Matrix4x4 matrixItemRotation = Matrix4x4.Rotate(itemRotation);
                Matrix4x4 matrixLocalPosition = Matrix4x4.Translate(localPosition);
                Draw(sceneView.camera, Matrix4x4.Translate(transform.position) * matrixItemRotation * matrixLocalPosition * matrixHandDirection * matrixHandModel);

                float valueX = (right ? (1f - handDirectionX) : -(1f - handDirectionX));
                Quaternion handDirX = Quaternion.Euler(valueX, 0f, 0f);
                Vector3 pos = transform.TransformPoint(localPosition);
                UnityEditor.Handles.DrawLine(pos, pos + (itemRotation * GetHandRotation(Quaternion.identity, right) * handDirection  * Vector3.back * 0.2f));
            }

        }

        void Draw(Camera camera, Matrix4x4 matrix) {
            if (mesh && material && camera) {
                Graphics.DrawMesh(mesh, matrix, material, gameObject.layer, camera);
            }
        }

        void OnDrawGizmos() {
            if (!MyEditorSettings.TypeSetting.AlwaysShowGhostHandInteraction.GetToggle()) return;
            render = true;
        }

        void OnDrawGizmosSelected() {
            render = true;
        }
#endif
        #endregion

    }

    public struct HandHandlerData {
        public Vector3 handPosition;
        public Quaternion handRotation;
        public Vector3 holdDirection;
        public Vector3 itemPosition;
        public Quaternion itemRotation;
        public float weight;
        public CharacterHandInteraction handPose;
    }

    public class HandHandlerBaseInitData {
        public Vector3 holdDirection;
        public float weight;
        public HandHandler.HoldType holdTypeForPosition = HandHandler.HoldType.None;
        public HandHandler.HoldType holdTypeForRotation = HandHandler.HoldType.None;
    }

    class HandHandlersPool<T> : CustomPool<T> where T : HandHandler {
        static HandHandlersPool<T> _pool;
        public static HandHandlersPool<T> pool {
            get {
                if (_pool == null) {
                    _pool = new HandHandlersPool<T>();
                }
                return _pool;
            }
        }
    }

}

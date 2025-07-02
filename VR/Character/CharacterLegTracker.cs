using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Kubeec.VR.Character {
    public class CharacterLegTracker : MonoBehaviour, IAction {

        public event Action onAction;

        [SerializeField] LayerMask groundMask;
        [SerializeField] float distanceFromMiddle;
        [SerializeField] float distanceYOffset;
        [SerializeField] float stepDistance = 0.5f;
        [SerializeField] float stepOffsetHeight = 0.05f;
        [SerializeField] float speedStep = 2f;
        [SerializeField] float kneeRiseHeight = 0.2f;
        [SerializeField] float velocityPositionMultiplier = 15f;

        bool isOnGround = true;

        public bool IsOnGround => isOnGround;

        Vector3 currentPosition;
        Vector3 lastPosition;
        Animator animator;
        float multiplier = 1f;
        AvatarIKGoal ikgoal;
        float lerp = 0f;
        Ray ray;
        Vector3 currentHitWorldPosition;

        void OnDrawGizmos() {
            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(currentPosition, Vector3.one * 0.1f);
            Gizmos.color = Color.red;
            Gizmos.DrawCube(lastPosition, Vector3.one * 0.1f);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(ray);
        }

        public void Init(Animator animator, AvatarIKGoal leg) {
            this.animator = animator;
            ikgoal = leg;
            multiplier = leg == AvatarIKGoal.LeftFoot ? -1f : 1f;
            currentHitWorldPosition = transform.position;
            stepDistance *= stepDistance;
        }

        [NonSerialized] RaycastHit[] _results;
        public void UpdateLeg(Vector3 position, Quaternion rotation, Vector3 velocity, float maxDistanceToGround, ref bool canMoveFromGround) {
            if (canMoveFromGround) {
                velocity = velocity * Time.deltaTime * velocityPositionMultiplier;
                currentPosition = velocity + position + Quaternion.Euler(0, rotation.eulerAngles.y, 0) * (Vector3.right * multiplier * distanceFromMiddle);
                currentPosition += Vector3.up * distanceYOffset;
                transform.rotation = rotation;
                //ray = new Ray(currentPosition, Vector3.down * maxDistanceToGround);
                /* if (Physics.RaycastNonAlloc(ray, _results, maxDistanceToGround, groundMask) > 0) {
                     float distance = (_results[0].point - currentHitWorldPosition).sqrMagnitude;
                     if (distance > stepDistance) {
                         lastPosition = currentHitWorldPosition;
                         currentHitWorldPosition = _results[0].point + Vector3.up * stepOffsetHeight;
                         StartWalk();
                         canMoveFromGround = false;
                     }
                 }*/
                Vector3 pos = currentPosition + (Vector3.down * maxDistanceToGround);
                pos.y = 0f;//temp todo
                float distance = (pos - currentHitWorldPosition).sqrMagnitude;
                if (distance > stepDistance + velocity.magnitude) {
                    lastPosition = currentHitWorldPosition;
                    currentHitWorldPosition = pos + Vector3.up * stepOffsetHeight;
                    StartWalk();
                    canMoveFromGround = false;
                }
            }
            UpdatePosition();
        }

        void UpdatePosition() {
            if (!isOnGround) {
                if (lerp > 1) {
                    isOnGround = true;
                    SetIKPosition(currentHitWorldPosition);
                    onAction?.Invoke();
                    return;
                }
                Vector3 newPosition = Vector3.Lerp(lastPosition, currentHitWorldPosition, lerp);
                newPosition.y += Mathf.Sin(lerp * Mathf.PI) * kneeRiseHeight;
                SetIKPosition(newPosition);
                lerp += Time.deltaTime * speedStep;
            } else {
                SetIKPosition(currentHitWorldPosition);
            }
        }

        void SetIKPosition(Vector3 position) {
            animator.SetIKPositionWeight(ikgoal, 1f);
            animator.SetIKPosition(ikgoal, position);
        }

        void StartWalk() {
            lerp = 0f;
            isOnGround = false;
        }

    }
}

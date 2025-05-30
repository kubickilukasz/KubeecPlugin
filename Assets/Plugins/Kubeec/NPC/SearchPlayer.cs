using Kubeec.NPC;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

public class SearchPlayer : MonoBehaviour{

    const float errorValueToPlayer = 0.05f;

    [SerializeField] float maxDistance = 10f;
    [SerializeField] float maxAngle = 45f;
    [SerializeField] LayerMask layerMaskObstacles;

    void OnDrawGizmosSelected() {
        if (!Application.isPlaying) {
            return;
        }
        foreach (PlayerReference player in NPCContainer.instance.Players) {
            foreach (Transform point in player.Points) {
                Vector3 dir = point.position - transform.position;
                Gizmos.DrawRay(transform.position, transform.forward);
                float angle = Vector3.Angle(dir, transform.forward);
                if (angle >= maxAngle) {
                    Gizmos.color = Color.red;
                } else if (dir.magnitude >= maxDistance) {
                    Gizmos.color = Color.yellow;
                } else {
                    Gizmos.color = Color.green;
                }

                Gizmos.DrawRay(transform.position, dir);
            }
        }
    }

    public PlayerReference FindClosest() {
        float minDistance = float.MaxValue;
        PlayerReference current = null;
        foreach (PlayerReference player in NPCContainer.instance.Players) {
            float dis = (player.transform.position - transform.position).sqrMagnitude;
            if (dis < minDistance) {
                current = player;
                minDistance = dis;
            }
        }
        return current;
    }

    public List<PlayerReference> Search(Vector3 viewDirection) {
        List<PlayerReference> potentials = new();
        foreach (PlayerReference player in NPCContainer.instance.Players) {
            Vector3 dir = player.transform.position - transform.position;
            float angle = Vector3.Angle(dir, viewDirection);
            if (dir.magnitude < maxDistance && angle < maxAngle) {
                potentials.Add(player);
            }
        }
        List<PlayerReference> visible = new();
        foreach (PlayerReference player in potentials) {
            if (CanSee(player)) {
                visible.Add(player);
            }
        }
        return visible;
    }

    public bool CanSee(PlayerReference player) {
        foreach (Transform point in player.Points) {
            Vector3 dir = point.position - transform.position;
            if (Physics.Raycast(transform.position, dir.normalized, out RaycastHit hit, dir.magnitude - errorValueToPlayer, layerMaskObstacles)) {
                //Debug.Log($"RaycastHit {hit.collider.name}");
            } else {
                return true;
            }
        }
        return false;
    }


}

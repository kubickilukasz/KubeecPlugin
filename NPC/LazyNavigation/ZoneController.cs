using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kubeec.NPC.LazyNavigation {

    public class ZoneController : MonoBehaviour {

        [SerializeField] List<Zone> zones = new List<Zone> ();
        [SerializeField] float maxDistanceToZone = 1.5f;

        [ContextMenu("AutoSetupZones")]
        public void SetupZones() {
            zones.Clear();
            Zone[] tempZones = GetComponentsInChildren<Zone>();
            foreach (Zone zone in tempZones) {
                zone.Clear();
            }
            for (int i = 0; i < tempZones.Length; i++) {
                Zone zone1 = tempZones[i];
                for (int j = i + 1; j < tempZones.Length; j++) {
                    Zone zone2 = tempZones[j];
                    Vector3 pos1 = zone1.ClosestPoint(zone2);
                    Vector3 pos2 = zone2.ClosestPoint(zone1);
                    float distance = Vector3.Distance(pos1, pos2);
                    if (distance <= maxDistanceToZone) {
                        zone1.AddNeighbour(zone2);
                        zone2.AddNeighbour(zone1);
                    }
                }
            }
            zones = tempZones.ToList();
#if UNITY_EDITOR
            if (!Application.isPlaying) {
                UnityEditor.EditorUtility.SetDirty(gameObject);
            }
#endif
        }

        public Zone FindZone(Vector3 position, bool findClosest = true) {
            Zone target = null;
            foreach (var zone in zones) {
                if (zone.IsInZone(position)) {
                    target = zone;
                    break;
                }
            }
            if (target == null && findClosest) {
                float sqrDis = float.MaxValue;
                foreach (var zone in zones) {
                    float currentDis = (zone.FindClosestPoint(position) - position).sqrMagnitude;
                    if (currentDis < sqrDis) {
                        sqrDis = currentDis;
                        target = zone;
                    }
                }
            }
            return target;
        }

        public List<Vector3> FindPath(Zone start, Zone target, Vector3? startPosition = null) {
            if (target.isBlocked) {
                return null;
            }
            List<Node> toCheck = new List<Node>();
            Dictionary<Zone, Node> existedNodes = new Dictionary<Zone,Node>();
            existedNodes.Add(start, new Node(start, null, 0));
            toCheck.Add(existedNodes[start]);

            while (toCheck.Count > 0) {
                Node current = FindLowestCost(toCheck);
                toCheck.Remove(current);
                if (current.zone == target) {
                    return GetPathPositions(current, startPosition);
                }
                if (current.zone.isBlocked) {
                    continue;
                }
                for (int i = 0; i < current.zone.Neighbours.Count; i++) {
                    Zone nZone = current.zone.Neighbours[i];
                    Node node = null;
                    if (!existedNodes.TryGetValue(nZone, out node)) {
                        node = new Node(nZone, current, GetCost(current, nZone, target));
                        existedNodes.Add(nZone, node);
                        toCheck.Add(node);
                    } else {
                        float currentCost = GetCost(current, nZone, target);
                        if (currentCost < node.actualCost) {
                            node.actualCost = currentCost;
                            node.cameFrom = current;
                            if (!toCheck.Contains(node)) {
                                toCheck.Add(node);
                            }
                        }
                    }
                }

            }

            return null;
        }

        Node FindLowestCost(List<Node> toCheck) {
            float currentCost = float.MaxValue;
            Node currentNode = null;
            foreach (Node node in toCheck) {
                if (node.actualCost < currentCost) {
                    currentCost = node.actualCost;
                    currentNode = node;
                }
            }
            return currentNode;
        }

        float GetCost(Node parent, Zone currnet, Zone target) {
            return parent.actualCost + (currnet.transform.position - target.transform.position).sqrMagnitude + currnet.Cost;
        }

        List<Zone> GetPath(Node target) {
            List<Zone> path = new List<Zone>();
            do  {
                path.Add(target.zone);
                target = target.cameFrom;
            } while (target != null);
            path.Reverse();
            return path;
        }

        List<Vector3> GetPathPositions(Node target, Vector3? startPosition = null) {
            List<Vector3> path = new List<Vector3>();
            List<Zone> pathZone = GetPath(target);
            //path.Add(target.zone.GetCenter());
            //Node cameFrom = target.cameFrom;
            //while (cameFrom != null) {
            //    pathZone.Add(target.zone);
            //    pathZone.Add(cameFrom.zone);
            //    path.Add(target.zone.ClosestPoint(cameFrom.zone));
            //    path.Add(cameFrom.zone.ClosestPoint(target.zone));
            //    target = cameFrom;
            //    cameFrom = target.cameFrom;
            //}
            //path.Reverse();
            Vector3 currentPosition = startPosition ?? pathZone[0].GetCenter();
            for (int i = 1; i < pathZone.Count; i++) {
                //path.Add(pathZone[i].FindClosestPoint(currentPosition));
                Vector3 closest = pathZone[i].FindClosestPoint(currentPosition);
                path.Add(closest);
                currentPosition = closest;
            }
            return path;
        }

        class Node {
            public Zone zone;
            public Node cameFrom;
            public float actualCost;

            public Node(Zone zone, Node cameFrom, float cost) {
                this.zone = zone;
                this.cameFrom = cameFrom;
                this.actualCost = cost;
            }
        }


    }

}

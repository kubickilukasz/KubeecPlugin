using System.Collections.Generic;
using UnityEngine;

namespace Kubeec.NPC.LazyNavigation {
    public class Zone : MonoBehaviour {

        public bool isBlocked = false;

        [SerializeField] Bounds bounds;
        [SerializeField] float cost = 0f;
        [SerializeField] List<Zone> neighbours = new();

        public float Cost => cost;
        public IList<Zone> Neighbours => neighbours;

        void OnValidate() {
            for (int i = 0; i < neighbours.Count; i++) {
                if (neighbours[i] != null && !neighbours[i].neighbours.Contains(this)) {
                    neighbours[i].neighbours.Add(this);
                }
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmos() {
            if (!MyEditorSettings.Instance.GetToggle(MyEditorSettings.TypeSetting.ShowGizmosLazyNavigation)) {
                return;
            }
            Gizmos.color = isBlocked ? Color.red : Color.yellow;
            Gizmos.DrawWireCube(GetCenter(), bounds.size);
            UnityEditor.Handles.Label(GetCenter(), $"{name}");
            foreach (var zone in neighbours) {
                if (zone == null) {
                    continue;
                }
                Vector3 start = zone.ClosestPoint(this);
                Vector3 end = ClosestPoint(zone);
                Gizmos.DrawLine(start, end);
            }
        }
#endif

        public void Clear() {
            neighbours.Clear();
        }

        public void AddNeighbour(Zone zone) {
            neighbours.Add(zone);
        }

        public Vector3 GetRandomPositionInZone() {
            return bounds.GetRandomPosition(transform.position);
        }

        public Vector3 GetRandomPositionInZoneOnFloor() {
            Bounds newBounds = bounds;
            newBounds.center = new Vector3(newBounds.center.x, -newBounds.size.y / 2f, newBounds.center.z);
            newBounds.size = new Vector3(newBounds.size.x, 0f, newBounds.size.z);
            return newBounds.GetRandomPosition(transform.position);
        }

        public Vector3 GetCenter() {
            return transform.position + bounds.center;
        }

        public Vector3 FindClosestPoint(Vector3 position) {
            Vector3 localPosition = position - transform.position;
            if (bounds.Contains(localPosition)) {
                return position;
            }
            return bounds.ClosestPoint(localPosition) + transform.position;
        }

        public Vector3 ClosestPoint(Zone zone) {
            if (zone == this) {
                return zone.GetCenter();
            }
            Vector3 localPosition = zone.GetCenter() - transform.position;
            return bounds.ClosestPoint(localPosition) + transform.position;
        }

        public bool IsInZone(Vector3 position) {
            Vector3 localPosition = position - transform.position;
            return bounds.Contains(localPosition);
        }

    }

}

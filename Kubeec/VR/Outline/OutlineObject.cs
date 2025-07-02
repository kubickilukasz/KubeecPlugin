using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kubeec.VR.Outline {
    [DisallowMultipleComponent]
    public class OutlineObject : MonoBehaviour {

        //[SerializeField] bool setupOnStart = true;
        [SerializeField] Renderer[] renderers;

        [SerializeField, HideInInspector]
        private List<Mesh> bakeKeys = new List<Mesh>();

        public HashSet<Mesh> registeredMeshes => OutlineManager.instanceExist ? OutlineManager.instance.registeredMeshes : null;

        [Serializable]
        private class ListVector3 {
            public List<Vector3> data;
        }

        [SerializeField, HideInInspector]
        private List<ListVector3> bakeValues = new List<ListVector3>();

        void Start() {
            //if (setupOnStart) {
                SetupRenderers();
            //}
        }

        public Renderer[] GetRenderers() {
            return renderers;
        }

        //[Button]
        void SetupRenderers() {
            //renderers = GetComponentsInChildren<Renderer>(true);
            LoadSmoothNormals();
        }


        void Bake() {
            var bakedMeshes = new HashSet<Mesh>();
            foreach (var meshFilter in GetComponentsInChildren<MeshFilter>()) {
                if (!bakedMeshes.Add(meshFilter.sharedMesh)) {
                    continue;
                }
                var smoothNormals = SmoothNormals(meshFilter.sharedMesh);
                bakeKeys.Add(meshFilter.sharedMesh);
                bakeValues.Add(new ListVector3() { data = smoothNormals });
            }
        }

        void LoadSmoothNormals() {
            List<MeshFilter> meshFilters = renderers.Select(x => x.GetComponent<MeshFilter>()).ToList();
            foreach (MeshFilter meshFilter in meshFilters) {
                if (registeredMeshes == null || !registeredMeshes.Add(meshFilter.sharedMesh)) {
                    continue;
                }

                var index = bakeKeys.IndexOf(meshFilter.sharedMesh);
                var smoothNormals = (index >= 0) ? bakeValues[index].data : SmoothNormals(meshFilter.sharedMesh);
                meshFilter.sharedMesh.SetUVs(3, smoothNormals);
                var renderer = meshFilter.GetComponent<Renderer>();

                if (renderer != null) {
                    CombineSubmeshes(meshFilter.sharedMesh, renderer.sharedMaterials);
                }
            }
            foreach (var skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>()) {
                if (registeredMeshes == null || !registeredMeshes.Add(skinnedMeshRenderer.sharedMesh)) {
                    continue;
                }
                skinnedMeshRenderer.sharedMesh.uv4 = new Vector2[skinnedMeshRenderer.sharedMesh.vertexCount];
                CombineSubmeshes(skinnedMeshRenderer.sharedMesh, skinnedMeshRenderer.sharedMaterials);
            }
        }

        List<Vector3> SmoothNormals(Mesh mesh) {

            var groups = mesh.vertices.Select((vertex, index) => new KeyValuePair<Vector3, int>(vertex, index)).GroupBy(pair => pair.Key);
            var smoothNormals = new List<Vector3>(mesh.normals);
            foreach (var group in groups) {
                if (group.Count() == 1) {
                    continue;
                }

                var smoothNormal = Vector3.zero;

                foreach (var pair in group) {
                    smoothNormal += smoothNormals[pair.Value];
                }

                smoothNormal.Normalize();
                foreach (var pair in group) {
                    smoothNormals[pair.Value] = smoothNormal;
                }
            }

            return smoothNormals;
        }

        void CombineSubmeshes(Mesh mesh, Material[] materials) {
            if (mesh.subMeshCount == 1) {
                return;
            }
            if (mesh.subMeshCount > materials.Length) {
                return;
            }

            mesh.subMeshCount++;
            mesh.SetTriangles(mesh.triangles, mesh.subMeshCount - 1);
        }

    }
}

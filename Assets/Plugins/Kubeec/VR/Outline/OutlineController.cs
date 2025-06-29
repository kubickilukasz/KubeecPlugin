using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Kubeec.VR.Outline {

    public class OutlineController : MonoBehaviour {

        //[SerializeField] BlurredBufferMultiObjectOutlineRendererFeature feature;
        Dictionary<MonoBehaviour, OutliningObjects> currentOutlinedObjects = new();
        OutlineManager outlineManager;

        void Start() {
            outlineManager = OutlineManager.instance;
        }

        void Update() {
            for (int i = 0; i < currentOutlinedObjects.Count; i++) {
                OutliningObjects current = currentOutlinedObjects.ElementAt(i).Value;
                if (!current.outline.CanOutline()) {
                    InternalStopOutline(current);
                    i--;
                }
            }
        }

        public void StartOutline(IOutlineable newOutline, MonoBehaviour reason, Vector3? position = null) {
            if (newOutline != null && reason != null && newOutline.CanOutline()) {
                if (currentOutlinedObjects.ContainsKey(reason)) {
                    if (currentOutlinedObjects[reason] == newOutline) {
                        return;
                    }
                    InternalStopOutline(currentOutlinedObjects[reason]);
                }
                OutliningObjects outliningObjects = new() {
                    reason = reason,
                    outline = newOutline,
                    objects = newOutline.GetOutlineObjects(position ?? reason.transform.position)
                };
                currentOutlinedObjects.Add(reason, outliningObjects);
                foreach (OutlineObject obj in outliningObjects.objects) {
                    foreach (Renderer r in obj.GetRenderers()) {
                        outlineManager.SetRenderer(r);
                    }
                }
            }
        }

        public void ForceStopOutline(MonoBehaviour reason) {
            if (currentOutlinedObjects.ContainsKey(reason)) {
                InternalStopOutline(currentOutlinedObjects[reason]);
            }
        }

        void InternalStopOutline(OutliningObjects outline) {
            foreach (OutlineObject obj in outline.objects) {
                foreach (Renderer r in obj.GetRenderers()) {
                    outlineManager.RemoveRenderer(r);
                }
            }
            currentOutlinedObjects.Remove(outline.reason);
            Refresh();
        }

        void Refresh() {
            foreach (var objs in currentOutlinedObjects) {
                foreach (OutlineObject obj in objs.Value.objects) {
                    foreach (Renderer r in obj.GetRenderers()) {
                        outlineManager.SetRenderer(r);
                    }
                }
            }
        }

        class OutliningObjects {
            public MonoBehaviour reason;
            public IOutlineable outline;
            public IEnumerable<OutlineObject> objects;
        }

    }
}


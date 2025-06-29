using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OutlineManager : Singleton<OutlineManager>{

    public HashSet<Mesh> registeredMeshes = new HashSet<Mesh>();

    [SerializeField]
    private Color outlineColor = Color.white;

    [SerializeField, Range(0f, 10f)]
    private float outlineWidth = 2f;

    Material outlineMaskMaterial;
    Material outlineFillMaterial;

    HashSet<Renderer> targetRenderers = new HashSet<Renderer>();

    void OnDestroy() {
        // Destroy material instances
        Destroy(outlineMaskMaterial);
        Destroy(outlineFillMaterial);
    }

    public void SetRenderer(Renderer targetRenderer) {
        if (targetRenderers.Contains(targetRenderer)) {
            return;
        }
        targetRenderers.Add(targetRenderer);
        AddMaterialToRenderer(targetRenderer);
    }

    public void RemoveRenderer(Renderer targetRenderer) {
        if (!targetRenderers.Contains(targetRenderer)) {
            return;
        }
        targetRenderers.Remove(targetRenderer);
        if (targetRenderer) {
            RemoveMaterialToRenderer(targetRenderer);
        }
    }

    protected override void OnInit() {
        outlineMaskMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlineMask"));
        outlineFillMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlineFill"));

        outlineMaskMaterial.name = "OutlineMask (Instance)";
        outlineFillMaterial.name = "OutlineFill (Instance)";

        UpdateMaterialProperties();
    }


    void AddMaterialToRenderer(Renderer renderer) {
        var materials = renderer.sharedMaterials.ToList();
        materials.Add(outlineMaskMaterial);
        materials.Add(outlineFillMaterial);
        renderer.materials = materials.ToArray();
    }

    void RemoveMaterialToRenderer(Renderer renderer) {
        var materials = renderer.sharedMaterials.ToList();
        materials.Remove(outlineMaskMaterial);
        materials.Remove(outlineFillMaterial);
        renderer.materials = materials.ToArray();
    }

    void UpdateMaterialProperties() {
        // Apply properties according to mode
        outlineFillMaterial.SetColor("_OutlineColor", outlineColor);
        outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
        outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
        outlineFillMaterial.SetFloat("_OutlineWidth", outlineWidth);
    }

}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BlurredBufferMultiObjectOutlineRendererFeature : ScriptableRendererFeature
{
    private static readonly int SpreadId = Shader.PropertyToID("_Spread");

    [SerializeField] private RenderPassEvent renderEvent = RenderPassEvent.AfterRenderingTransparents;
    [Space, SerializeField] private Material dilationMaterial;
    [SerializeField] private Material outlineMaterial;
    [SerializeField, Range(1, 60)] private int spread = 15;

    private BlurredBufferMultiObjectOutlinePass _outlinePass;

    private HashSet<Renderer> _targetRenderers = new HashSet<Renderer>();

    public void SetRenderer(Renderer targetRenderer) {
        if (_targetRenderers.Contains(targetRenderer)) {
            return;
        }

        _targetRenderers.Add(targetRenderer);
        SetRenderers(_targetRenderers);
    }

    public void RemoveRenderer(Renderer targetRenderer) {
        if (!_targetRenderers.Contains(targetRenderer)) {
            return;
        }
        _targetRenderers.Remove(targetRenderer);
        SetRenderers(_targetRenderers);
    }

    void SetRenderers(Renderer[] targetRenderers){
        _targetRenderers = new HashSet<Renderer>(targetRenderers);

        if (_outlinePass != null)
            _outlinePass.Renderers = targetRenderers;
    }

    void SetRenderers(HashSet<Renderer> targetRenderers) {
        _targetRenderers = targetRenderers;

        if (_outlinePass != null)
            _outlinePass.Renderers = targetRenderers.ToArray();
    }

    public override void Create()
    {
        name = "Multi-Object Outliner";

        // Pass in constructor variables which don't/shouldn't need to be updated every frame.
        _outlinePass = new BlurredBufferMultiObjectOutlinePass();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (_outlinePass == null)
            return;

        if (!dilationMaterial ||
            !outlineMaterial ||
            _targetRenderers == null ||
            _targetRenderers.Count == 0)
        {
            // Don't render the effect if there's nothing to render
            return;
        }

        // Any variables you may want to update every frame should be set here.
        _outlinePass.RenderEvent = renderEvent;
        _outlinePass.DilationMaterial = dilationMaterial;
        dilationMaterial.SetInteger("_Spread", spread);
        _outlinePass.OutlineMaterial = outlineMaterial;
        _outlinePass.Renderers = _targetRenderers.ToArray();

        renderer.EnqueuePass(_outlinePass);
    }
}
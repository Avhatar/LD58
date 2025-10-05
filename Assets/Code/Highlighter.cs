using System.Collections.Generic;
using UnityEngine;

public class Highlighter : MonoBehaviour
{
    private readonly int _outlineColor = Shader.PropertyToID("_OutlineColor");

    private class HighlightedRenderer
    {
        public Renderer                    OriginalRenderer;
        public MaterialPropertyBlock       MaterialPropertyBlock;
        public List<MaterialOriginalColor> OriginalMaterials;
    }

    private class MaterialOriginalColor
    {
        public Material OriginalMaterial;
        public Color    OriginalColor;
        public int      MatIndex;
    }
    
    private enum HighlighterState
    {
        On, Off, StandBy
    }

    [SerializeField]
    private HighlighterState _state = HighlighterState.StandBy;
    private List<HighlightedRenderer> _highlightedRenderers = new List<HighlightedRenderer>();
    private Color                     _highlightColor;
    private Color                     _highlightNewObjectColor;

    public void Init()
    {
        _highlightColor = Core.instance.globalSettings.characterSelectColor;
        
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        
        foreach (Renderer rend in renderers)
        {
            HighlightedRenderer newHighlightedRenderer = new HighlightedRenderer();
            newHighlightedRenderer.OriginalRenderer = rend;
            newHighlightedRenderer.MaterialPropertyBlock = new MaterialPropertyBlock();
            newHighlightedRenderer.OriginalMaterials = new List<MaterialOriginalColor>();
            for (var index = 0; index < rend.sharedMaterials.Length; index++)
            {
                var mat = rend.sharedMaterials[index];
                if (mat == null)
                    continue;
                MaterialOriginalColor matOriginalColor = new MaterialOriginalColor()
                {
                    OriginalMaterial = mat,
                    OriginalColor = mat.GetColor(_outlineColor),
                    MatIndex = index
                };
                newHighlightedRenderer.OriginalMaterials.Add(matOriginalColor);
            }

            _highlightedRenderers.Add(newHighlightedRenderer);
        }
    }
    
    public void ConstantOn()
    {
        if (_state == HighlighterState.On)
            return;
        Color hColor = _highlightColor;
        On(hColor);
        _state = HighlighterState.On;
    }

    private void On(Color color)
    {
        foreach (var highlightedRenderer in _highlightedRenderers)
        {
            highlightedRenderer.MaterialPropertyBlock.SetColor(_outlineColor, color);
            foreach (var mat in highlightedRenderer.OriginalMaterials)
            {
                highlightedRenderer.OriginalRenderer.SetPropertyBlock(highlightedRenderer.MaterialPropertyBlock, mat.MatIndex);
            }
        }
    }
    
    public void ConstantOnCustomColor(Color color)
    {
        if (_state == HighlighterState.On)
            return;
        Color hColor = color;
        On(hColor);
        _state = HighlighterState.On;
    }
    
    public void ConstantOff()
    {
        if (_state == HighlighterState.Off)
            return;
        foreach (var highlightedRenderer in _highlightedRenderers)
        {
            highlightedRenderer.MaterialPropertyBlock.SetColor(_outlineColor, highlightedRenderer.OriginalMaterials[0].OriginalColor);
            foreach (var mat in highlightedRenderer.OriginalMaterials)
            {
                highlightedRenderer.OriginalRenderer.SetPropertyBlock(highlightedRenderer.MaterialPropertyBlock, mat.MatIndex);
            }
        }
        _state = HighlighterState.Off;
    }
}
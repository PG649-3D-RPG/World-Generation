using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MaskEffect : MonoBehaviour{
    [Header("Mask Selection")]
    public TerrainMasksE mask = TerrainMasksE.LevelsCorridors;
    public int type = -1;
    public int invertedBorderMaskWidth = 0;
    public bool subtractBorder = false;
    public bool invertMask = false;
    public bool maskNull = false;

    protected abstract void Apply(Heightmap h, Mask mask);

    public void Apply(Heightmap h, TerrainMasks tmasks){
        Mask m = tmasks.MaskByEnum(mask, type : type);
        if(invertedBorderMaskWidth > -1 && subtractBorder) m = m.SubtractedBorderMask(invertedBorderMaskWidth); 
        else if(invertedBorderMaskWidth > -1) m = m.InvertedBorderMask(invertedBorderMaskWidth);    
        if(invertMask) m.Invert();
        if(maskNull) m = null;
        Apply(h, m);
    }
}
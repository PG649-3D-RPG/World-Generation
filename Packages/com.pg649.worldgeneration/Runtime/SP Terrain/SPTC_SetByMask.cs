using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPTC_SetByMask : MaskEffect
{
    [Header("Settings")]
    public float trueValue = 0f;
    public float falseValue = -1f;

    protected override void Apply(Heightmap h, Mask mask){
        h.SetByMask(mask, trueValue, falseValue : falseValue);
    }
}

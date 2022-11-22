using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPTC_Power : MaskEffect
{
    [Header("Settings")]
    public float power = 2f;

    protected override void Apply(Heightmap h, Mask mask){
        h.Power(power, mask);
    }
}
